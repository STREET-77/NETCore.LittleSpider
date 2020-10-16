using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider
{
	public class SpiderOptions
	{
		/// <summary>
		/// 请求队列数限制
		/// </summary>
		public int RequestedQueueCount { get; set; } = 1000;

		/// <summary>
		/// 请求重试次数限制
		/// </summary>
		public int RetriedTimes { get; set; } = 3;

		/// <summary>
		/// 爬虫采集速度，1 表示 1 秒钟一个请求，0.5 表示 1 秒钟 0.5 个请求，5 表示 1 秒钟发送 5 个请求
		/// </summary>
		public double Speed { get; set; } = 1;

		/// <summary>
		/// 一次重请求队列获取多少个请求
		/// </summary>
		public uint Batch { get; set; } = 4;
	}
}
