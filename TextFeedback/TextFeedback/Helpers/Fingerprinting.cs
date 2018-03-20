using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TextFeedback
{
	public static class Fingerprinting
	{
		private const string CookieName = "cookiejar";

		public static void AddFingerprint(this HttpResponseMessage response, string fingerprint)
		{
			response.Headers.AddCookies(new List<CookieHeaderValue> {
					new CookieHeaderValue(CookieName, fingerprint)
					{
						Expires = DateTimeOffset.Now.AddYears(10)
					}
				});
		}

		public static string GetFingerprint(this HttpRequestMessage request)
		{
			IEnumerable<CookieState> cookies = request.Headers.GetCookies()
											.SelectMany(x => x.Cookies)
											.Where(x =>
												x.Name.Equals(CookieName, StringComparison.OrdinalIgnoreCase) &&
												!string.IsNullOrWhiteSpace(x.Value));
			if (cookies.Any())
			{
				return cookies.FirstOrDefault().Value;
			}

			return Guid.NewGuid().ToString();
		}
	}
}