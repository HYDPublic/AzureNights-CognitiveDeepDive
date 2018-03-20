using System;

namespace VoiceAuth.Functions
{
	public class Profile
	{
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public Guid ProfileID { get; set; }
	}
}