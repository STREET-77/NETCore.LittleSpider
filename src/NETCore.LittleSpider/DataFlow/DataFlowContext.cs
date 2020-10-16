﻿using NETCore.LittleSpider.Http;
using NETCore.LittleSpider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NETCore.LittleSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器上下文
	/// </summary>
	public class DataFlowContext : IDisposable
	{
		private readonly Dictionary<string, dynamic> _properties = new Dictionary<string, dynamic>();
		private readonly Dictionary<object, dynamic> _data = new Dictionary<object, dynamic>();

		/// <summary>
		/// 爬虫标识
		/// </summary>
		public string Id { get; internal set; }

		/// <summary>
		/// 爬虫名称
		/// </summary>
		public string Name { get; internal set; }

		public SpiderOptions Options { get; }

		/// <summary>
		/// 下载器返回的结果
		/// </summary>
		public Response Response { get; }

		/// <summary>
		/// 消息队列回传的内容
		/// </summary>
		public byte[] MessageBytes { get; internal set; }

		/// <summary>
		/// 下载的请求
		/// </summary>
		public Request Request { get; }

		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response">下载器返回的结果</param>
		/// <param name="options"></param>
		/// <param name="serviceProvider"></param>
		public DataFlowContext(IServiceProvider serviceProvider,
			SpiderOptions options,
			Request request,
			Response response
		)
		{
			Request = request;
			Response = response;
			Options = options;
			ServiceProvider = serviceProvider;
		}

		/// <summary>
		/// 获取属性
		/// </summary>
		/// <param name="key">Key</param>
		public dynamic this[string key]
		{
			get => _properties.ContainsKey(key) ? _properties[key] : null;
			set
			{
				if (_properties.ContainsKey(key))
				{
					_properties[key] = value;
				}

				else
				{
					_properties.Add(key, value);
				}
			}
		}

		/// <summary>
		/// 是否包含属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns></returns>
		public bool Contains(string key)
		{
			return _properties.ContainsKey(key);
		}

		/// <summary>
		/// 添加属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		public void Add(string key, dynamic value)
		{
			_properties.Add(key, value);
		}

		/// <summary>
		/// 添加数据项
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="data">Value</param>
		public void AddData(object name, dynamic data)
		{
			if (_data.ContainsKey(name))
			{
				_data[name] = data;
			}

			else
			{
				_data.Add(name, data);
			}
		}

		/// <summary>
		/// 获取数据项
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns></returns>
		public dynamic GetData(object name)
		{
			return _data.ContainsKey(name) ? _data[name] : null;
		}

		/// <summary>
		/// 获取所有数据项
		/// </summary>
		/// <returns></returns>
		public IDictionary<object, dynamic> GetData()
		{
			return _data.ToImmutableDictionary();
		}

		/// <summary>
		/// 是否包含数据项
		/// </summary>
		public bool IsEmpty => _data.Count == 0;

		/// <summary>
		/// 清空数据
		/// </summary>
		public void Clear()
		{
			_data.Clear();
		}

		public void Dispose()
		{
			_properties.Clear();
			_data.Clear();
			MessageBytes = null;

			ObjectUtilities.DisposeSafely(Request);
			ObjectUtilities.DisposeSafely(Response);
		}
	}
}
