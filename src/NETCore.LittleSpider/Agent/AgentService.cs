using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.LittleSpider.Downloader;
using NETCore.LittleSpider.Extensions;
using NETCore.LittleSpider.Http;
using NETCore.LittleSpider.Infrastructure;
using NETCore.LittleSpider.MessageQueue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Agent
{
	public class AgentService : BackgroundService
	{
		private readonly ILogger _logger;
		private readonly IMessageQueue _messageQueue;
		private AsyncMessageConsumer<byte[]> _consumers;
		private readonly IDownloader _downloader;
		private readonly AgentOptions _options;

		public AgentService(ILogger<AgentService> logger,
			IMessageQueue messageQueue,
			IOptions<AgentOptions> options,
			IDownloader downloader, HostBuilderContext hostBuilderContext)
		{
			_options = options.Value;
			_logger = logger;
			_messageQueue = messageQueue;
			_downloader = downloader;

			hostBuilderContext.Properties[Const.DefaultDownloader] = downloader.Name;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Agent is starting");
			// 节点注册对应的 topic 才会收到下载的请求
			// agent_{id} 这是用于指定节点下载
			// httpclient 这是指定下载器
			await RegisterAgentAsync(_downloader.Name, stoppingToken);
			_logger.LogInformation("Agent started");
		}

		private async Task RegisterAgentAsync(string topic, CancellationToken stoppingToken)
		{
            _consumers = new MessageQueue.AsyncMessageConsumer<byte[]>(topic);
			_consumers.Received += HandleMessageAsync;
			await _messageQueue.ConsumeAsync(_consumers, stoppingToken);
		}

		private async Task HandleMessageAsync(byte[] bytes)
		{
			var message = await GetMessageAsync(bytes);
			if (message == null)
			{
				return;
			}

			if (message is Request request)
			{
				Task.Factory.StartNew(async () =>
				{
					var response = await _downloader.DownloadAsync(request);
					if (response == null)
					{
						return;
					}

					response.Agent = _options.AgentId;

					var topic = string.Format(Const.Topic.Spider, request.Owner.ToUpper());
					await _messageQueue.PublishAsBytesAsync(topic, response);

					_logger.LogInformation($"{request.Owner} download {request.RequestUri}, {request.Hash} completed");
				}).ConfigureAwait(false).GetAwaiter();
			}
			else
			{
				var msg = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(message));
				_logger.LogWarning($"Not supported message: {msg}");
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Agent is stopping");

			_consumers.Close();

			await base.StopAsync(cancellationToken);

			_logger.LogInformation("Agent stopped");
		}

		private async Task<object> GetMessageAsync(byte[] bytes)
		{
			try
			{
				return await bytes.DeserializeAsync();
			}
			catch (Exception e)
			{
				_logger.LogError($"Deserialize message failed: {e}");
				return null;
			}
		}
	}
}
