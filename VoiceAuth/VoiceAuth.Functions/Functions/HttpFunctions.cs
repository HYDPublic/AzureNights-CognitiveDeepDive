using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace VoiceAuth.Functions
{
	public class HttpFunctions
	{
		private const string EnrollmentTable = "enrollments";
		private const string EnrollmentRowKey = "enroll";

		private static readonly ISpeech _speech = new Speech();

		[FunctionName(nameof(Enroll))]
		public static async Task<HttpResponseMessage> Enroll(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enroll/{username:alpha}")]
				HttpRequestMessage req,
			string username,
			[Table(EnrollmentTable, "{username}", EnrollmentRowKey)]
				Profile verificationProfile,
			[Table(EnrollmentTable)]
				IAsyncCollector<Profile> profileOutput)
		{
			// Check to see that the incomming request isn't too large
			if (!req.Content.Headers.ContentLength.HasValue ||
				req.Content.Headers.ContentLength.Value > 5000000)
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			// If the user has no voice profile, create one
			if (verificationProfile == null)
			{
				SpeechProfile profile = await _speech.CreateProfileAsync();

				verificationProfile = new Profile
				{
					PartitionKey = username,
					RowKey = EnrollmentRowKey,
					ProfileID = profile.VerificationProfileId
				};

				await profileOutput.AddAsync(verificationProfile);
			}

			// Enroll the current profile with this speech
			byte[] speech = await req.Content.ReadAsByteArrayAsync();

			Enrollment status;

			try
			{
				status = await _speech.CreateEnrollmentAsync(verificationProfile.ProfileID, speech);
			}
			catch (SpeechException e)
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					ReasonPhrase = e.Error.Error.Code,
					Content = new StringContent(e.Error.Error.Message)
				};
			}

			HttpStatusCode statusCode = HttpStatusCode.Accepted;
			if (status.EnrollmentStatus == EnrollmentStatus.Enrolled)
			{
				statusCode = HttpStatusCode.OK;
			}

			// Return the status code
			return new HttpResponseMessage(statusCode)
			{
				ReasonPhrase = statusCode.ToString(),
				Content = new StringContent(statusCode.ToString())
			};
		}

		[FunctionName(nameof(Login))]
		public static async Task<HttpResponseMessage> Login(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login/{username:alpha}")]
				HttpRequestMessage req,
			string username,
			[Table(EnrollmentTable, "{username}", EnrollmentRowKey)]
				Profile verificationProfile)
		{
			// Check to see that the incomming request isn't too large
			if (!req.Content.Headers.ContentLength.HasValue ||
				req.Content.Headers.ContentLength.Value > 5000000)
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			// If we can't find an enrollment id, then they don't exist!
			if (verificationProfile == null)
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			// Verify the users profile
			byte[] speech = await req.Content.ReadAsByteArrayAsync();

			Verification profile = await _speech.VerifyAsync(verificationProfile.ProfileID, speech);

			// Build a message for the user
			string message = "";
			HttpStatusCode statusCode;
			if (profile.Result == VerificationResult.Reject)
			{
				message = "Your voice has been rejected, access denied!";
				statusCode = HttpStatusCode.Unauthorized;
			}
			else if (profile.Confidence == VerificationConfidence.Low)
			{
				message = "Your voice was identified, but confidence was low, access denied!";
				statusCode = HttpStatusCode.Unauthorized;
			}
			else
			{
				message = "Welcome " + username;
				statusCode = HttpStatusCode.OK;
			}

			// Return the HTTP call
			return new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(message)
			};
		}
	}
}