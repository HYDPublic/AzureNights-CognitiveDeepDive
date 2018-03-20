using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VoiceAuth.Functions
{
	public class Speech : ISpeech, IDisposable
	{
		private const string SubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

		private readonly HttpClient _httpClient;
		private readonly string _speakerRecognitionUrl;
		private readonly string _speakerRecognitionKey;

		public Speech()
		{
			_httpClient = new HttpClient();

			// Ideally passing through configuration in the ctor, but this will do for a demo!
			_speakerRecognitionUrl = ConfigurationManager.AppSettings["SpeakerRecognitionUrl"] + "/";
			_speakerRecognitionKey = ConfigurationManager.AppSettings["SpeakerRecognitionKey"];
		}

		public async Task<SpeechProfile> CreateProfileAsync()
		{
			SpeechProfileRequest request = new SpeechProfileRequest
			{
				Locale = "en-us"
			};

			StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

			return await PostAsync<SpeechProfile>(content, _speakerRecognitionUrl + "verificationProfiles");
		}

		public async Task<Enrollment> CreateEnrollmentAsync(Guid id, byte[] speech)
		{
			return await PostSpeechAsync<Enrollment>(speech, _speakerRecognitionUrl + "verificationProfiles/" + id + "/enroll");
		}

		public async Task<Verification> VerifyAsync(Guid id, byte[] speech)
		{
			return await PostSpeechAsync<Verification>(speech, _speakerRecognitionUrl + "verify?verificationProfileId=" + id);
		}

		private async Task<T> PostSpeechAsync<T>(byte[] speech, string url)
		{
			ByteArrayContent byteContent = new ByteArrayContent(speech);
			byteContent.Headers.Remove("Content-Type");
			byteContent.Headers.Add("Content-Type", "application/octet-stream");

			return await PostAsync<T>(byteContent, url);
		}

		private async Task<T> PostAsync<T>(HttpContent content, string url)
		{
			HttpRequestMessage sentimentRequest = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = content
			};
			sentimentRequest.Headers.Add(SubscriptionKeyHeader, _speakerRecognitionKey);

			HttpResponseMessage response = await _httpClient.SendAsync(sentimentRequest);

			string body = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				throw new SpeechException(JsonConvert.DeserializeObject<SpeechError>(body));
			}

			return JsonConvert.DeserializeObject<T>(body);
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}