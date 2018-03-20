namespace VoiceAuth.Functions
{
	public class Verification
	{
		public VerificationResult Result { get; set; }
		public VerificationConfidence Confidence { get; set; }
		public string Phrase { get; set; }
	}
}