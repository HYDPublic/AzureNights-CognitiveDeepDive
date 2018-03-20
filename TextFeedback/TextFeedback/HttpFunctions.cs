using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace TextFeedback
{
	public class HttpFunctions
	{
		private static readonly ITextAnalytics _textAnalytics = new TextAnalytics();

		[FunctionName(nameof(SubmitText))]
		public static async Task<HttpResponseMessage> SubmitText(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
				HttpRequestMessage request)
		{
			string fingerprint = request.GetFingerprint();

			// Detect sentiment
			string body = await request.Content.ReadAsStringAsync();
			double score = await _textAnalytics.GetSentimentAsync(body);

			// Detect Key Phrases
			string[] phrases = await _textAnalytics.GetKeyPhrasesAsync(body);

			// Ship some data off here!
			// Maybe log to a CRM?

			// Build a response
			string responseMessage = BuildMessage(score, phrases);

			// Send our response back
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseMessage)
			};
			response.AddFingerprint(fingerprint);

			return response;
		}

		private static string BuildMessage(double score, string[] keyPhrases)
		{
			StringBuilder responseMessage = new StringBuilder("Thanks for the ");

			if (score < 0.3) responseMessage.Append("constructive feedback.");
			else if (score > 0.7) responseMessage.Append("awesome feedback!");
			else responseMessage.Append("feedback.");

			if (keyPhrases.Length > 0)
			{
				if (score < 0.3) responseMessage.Append(" We will look into the ");
				else if (score > 0.7) responseMessage.Append(" We are proud of our ");
				else responseMessage.Append(" We are glad you enjoyed the ");

				for (int i = 0; i < keyPhrases.Length; i++)
				{
					responseMessage.Append(keyPhrases[i].ToLower());
					if (i < keyPhrases.Length - 1 && keyPhrases.Length > 2)
					{
						responseMessage.Append(", ");
					}
					if (i == keyPhrases.Length - 2)
					{
						if (keyPhrases.Length == 2)
						{
							responseMessage.Append(" ");
						}
						responseMessage.Append("and ");
					}
				}
				responseMessage.Append(".");
			}

			return responseMessage.ToString();
		}
	}
}