using System.Threading.Tasks;

namespace TextFeedback
{
	public interface ITextAnalytics
	{
		Task<double> GetSentimentAsync(string text);

		Task<string[]> GetKeyPhrasesAsync(string text);
	}
}