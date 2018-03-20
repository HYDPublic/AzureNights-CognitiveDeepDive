using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace IoTHubDashboard
{
	public class StaticFileFunctions
	{
		private const bool CacheEnabled = false;

		[FunctionName(nameof(StaticDevicePage))]
		public static HttpResponseMessage StaticDevicePage(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/login.html")] HttpRequestMessage req) => GetStaticFile("login.html", "text/html");

		[FunctionName(nameof(StaticRecorderJS))]
		public static HttpResponseMessage StaticRecorderJS(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/recorder.js")] HttpRequestMessage req) => GetStaticFile("recorder.js", "text/html");

		[FunctionName(nameof(StaticDashboardPage))]
		public static HttpResponseMessage StaticDashboardPage(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/register.html")] HttpRequestMessage req) => GetStaticFile("register.html", "text/html");

		private static HttpResponseMessage GetStaticFile(string name, string type) => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(CacheEnabled ? GetCachedFile(name) : LoadFile(name), Encoding.UTF8, type)
		};

		private static readonly ConcurrentDictionary<string, string> MiniCache = new ConcurrentDictionary<string, string>();

		private static string GetCachedFile(string name) => MiniCache.GetOrAdd(name, LoadFile);

		private static string LoadFile(string name) => File.ReadAllText(GetAppDataPath() + @"\" + name, Encoding.UTF8);

		private static string GetAppDataPath() => Path.Combine(GetEnvironmentVariable("HOME"), @"site\wwwroot\App_Data");

		private static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
	}
}