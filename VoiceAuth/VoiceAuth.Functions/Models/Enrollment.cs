namespace VoiceAuth.Functions
{
	public class Enrollment
	{
		public EnrollmentStatus EnrollmentStatus { get; set; }
		public int EnrollmentsCount { get; set; }
		public int RemainingEnrollments { get; set; }
		public string Phrase { get; set; }
	}
}