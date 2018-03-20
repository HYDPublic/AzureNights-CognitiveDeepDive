using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Newtonsoft.Json;

namespace TextFeedback
{
	public class TextAnalytics : ITextAnalytics, IDisposable
	{
		private const string SubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

		private readonly HttpClient _httpClient;
		private readonly string _textAnalyticsKey;
		private readonly string _textAnalyticsUrl;

		public TextAnalytics()
		{
			_httpClient = new HttpClient();

			// Ideally should take a settings dependency in here!
			_textAnalyticsUrl = ConfigurationManager.AppSettings["TextAnalyticsUrl"] + "/";
			_textAnalyticsKey = ConfigurationManager.AppSettings["TextAnalyticsKey"];
		}

		public async Task<double> GetSentimentAsync(string text)
		{
			// Build our request model
			MultiLanguageBatchInput requestDocument = GetSingleInput(text);

			// Get the result
			SentimentBatchResult result = await PostSentimentRequestAsync(requestDocument);

			// Return the single sentiment value
			return result.Documents.Single().Score.Value;
		}

		public async Task<string[]> GetKeyPhrasesAsync(string text)
		{
			// Build our request model
			MultiLanguageBatchInput requestDocument = GetSingleInput(text);

			// Get the result
			KeyPhraseBatchResult result = await PostKeyPhrasesRequestAsync(requestDocument);

			// Return an array of all the key phrases
			return result.Documents.SelectMany(x => x.KeyPhrases).ToArray();
		}

		private static MultiLanguageBatchInput GetSingleInput(string text) =>
			new MultiLanguageBatchInput
			{
				Documents = new List<MultiLanguageInput>
					{
						new MultiLanguageInput
						{
							Id = "0",
							Text = text,
							Language = "en"
						}
					}
			};

		private Task<SentimentBatchResult> PostSentimentRequestAsync(MultiLanguageBatchInput input) =>
			PostAnalyticsRequestAsync<SentimentBatchResult>(input, _textAnalyticsUrl + "sentiment");

		private Task<KeyPhraseBatchResult> PostKeyPhrasesRequestAsync(MultiLanguageBatchInput input) =>
			PostAnalyticsRequestAsync<KeyPhraseBatchResult>(input, _textAnalyticsUrl + "keyPhrases");

		private async Task<T> PostAnalyticsRequestAsync<T>(object input, string uri)
		{
			// Build a post request with our body & key header
			HttpRequestMessage sentimentRequest = new HttpRequestMessage(HttpMethod.Post, uri)
			{
				Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json")
			};
			sentimentRequest.Headers.Add(SubscriptionKeyHeader, _textAnalyticsKey);

			// Send the request
			HttpResponseMessage response = await _httpClient.SendAsync(sentimentRequest);

			// Grab the response body
			string json = await response.Content.ReadAsStringAsync();

			// Return our model
			return JsonConvert.DeserializeObject<T>(json);
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}