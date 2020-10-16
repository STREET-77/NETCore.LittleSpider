namespace NETCore.LittleSpider.Infrastructure
{
    public static class Const
	{
		public const string ProxyPrefix = "LittleSpider_Proxy_";
		public const string IgnoreSslError = "LittleSpider_Ignore_SSL_Error";
		public const string DefaultDownloader = "LittleSpider_Default_Downloader";

		public static class Downloader
		{
			public const string HttpClient = "LittleSpider_HttpClient_Downloader";
			public const string ProxyHttpClient = "LittleSpider_Proxy_HttpClient_Downloader";
			public const string FakeHttpClient = "LittleSpider_Fake_HttpClient_Downloader";
			public const string FakeProxyHttpClient = "LittleSpider_Fake_Proxy_HttpClient_Downloader";

			public const string Puppeteer = "LittleSpider_Puppeteer_Downloader";
			public const string File = "LittleSpider_File_Downloader";
			public const string Empty = "LittleSpider_Empty_Downloader";
		}

		public static class Topic
		{
			public const string AgentCenter = "LittleSpider_Agent_Center";
			public const string Statistics = "LittleSpider_Statistics_Center";
			public const string Spider = "LittleSpider_{0}";
		}

		public static class EnvironmentNames
		{
			public const string EntityIndex = "ENTITY_INDEX";
			public const string Guid = "GUID";
			public const string Date = "DATE";
			public const string Today = "TODAY";
			public const string Datetime = "DATETIME";
			public const string Now = "NOW";
			public const string Month = "MONTH";
			public const string Monday = "MONDAY";
			public const string SpiderId = "SPIDER_ID";
			public const string RequestHash = "REQUEST_HASH";
		}
	}
}
