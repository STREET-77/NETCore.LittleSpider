using NETCore.LittleSpider.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Downloader
{
	public interface IDownloader
	{
		Task<Response> DownloadAsync(Request request);

		string Name { get; }
	}
}
