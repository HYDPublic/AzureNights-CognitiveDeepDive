using System;

namespace VoiceAuth.Functions
{
	public class SpeechException : Exception
	{
		public SpeechError Error { get; }

		public SpeechException(SpeechError error)
			: base(error.Error.Message)
		{
			Error = error;
		}
	}
}