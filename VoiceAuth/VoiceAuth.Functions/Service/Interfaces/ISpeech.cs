using System;
using System.Threading.Tasks;

namespace VoiceAuth.Functions
{
	public interface ISpeech
	{
		Task<SpeechProfile> CreateProfileAsync();

		Task<Enrollment> CreateEnrollmentAsync(Guid id, byte[] speech);

		Task<Verification> VerifyAsync(Guid id, byte[] speech);
	}
}