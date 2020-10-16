using Bert.RateLimiters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.LittleSpider.DataFlow;
using NETCore.LittleSpider.Extensions;
using NETCore.LittleSpider.Http;
using NETCore.LittleSpider.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider
{
    public class SpiderFactory : IDisposable
    {
        private readonly List<IDataFlow> _dataFlows;
        private MessageQueue.AsyncMessageConsumer<byte[]> _consumer;
        private readonly RequestedQueue _requestedQueue;
        private readonly DependenceServices _services;
        private readonly string _defaultDownloader;

        public event Action<Request[]> OnTimeout;

        public event Action<Request, Response> OnError;

        public event Action OnSchedulerEmpty;

        private SpiderOptions Options { get; set; }

        /// <summary>
        /// 爬虫标识
        /// </summary>
        private string Id { get; set; }

        /// <summary>
        /// 爬虫名称
        /// </summary>
        public string Name { get; private set; }

        private ILogger Logger { get; set; }

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public SpiderFactory(IOptions<SpiderOptions> options,
            DependenceServices services,
            ILogger<SpiderFactory> logger
        )
        {
            Logger = logger;
            Options = options.Value;
            Id = Guid.NewGuid().ToString("N");

            if (Options.Speed > 500)
            {
                throw new ArgumentException("Speed should not large than 500");
            }

            _services = services;
            _dataFlows = new List<IDataFlow>();
            _requestedQueue = new RequestedQueue();

            _defaultDownloader = _services.HostBuilderContext.Properties.ContainsKey(Const.DefaultDownloader)
                ? _services.HostBuilderContext.Properties[Const.DefaultDownloader]?.ToString()
                : Const.Downloader.HttpClient;
        }

        /// <summary>
        /// 获取爬虫标识和名称
        /// </summary>
        /// <returns></returns>
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        private void Stop()
        {
            Logger.LogInformation($"{Id} stopping");

            _consumer?.Close();
            _services.MessageQueue.CloseQueue(Id);

            Dispose();

            Logger.LogInformation($"{Id} stopped");
        }

        public SpiderFactory AddDataFlow(IDataFlow dataFlow)
        {
            if (dataFlow == null)
                throw new ArgumentNullException(nameof(dataFlow));
            _dataFlows.Add(dataFlow);
            return this;
        }

        public async Task<int> AddRequestsAsync(params string[] requests)
        {
            if (requests == null || requests.Length == 0)
            {
                return 0;
            }

            return await AddRequestsAsync(requests.Select(x => new Request(x)));
        }

        public async Task<int> AddRequestsAsync(params Request[] requests)
        {
            if (requests == null || requests.Length == 0)
            {
                return 0;
            }

            return await AddRequestsAsync(requests.AsEnumerable());
        }

        public async Task<int> AddRequestsAsync(IEnumerable<Request> requests)
        {
            if (requests == null)
            {
                return 0;
            }

            var list = new List<Request>();

            foreach (var request in requests)
            {
                if (string.IsNullOrWhiteSpace(request.Downloader)
                    && !string.IsNullOrWhiteSpace(_defaultDownloader))
                {
                    request.Downloader = _defaultDownloader;
                }

                request.RequestedTimes += 1;

                // 1. 请求次数超过限制则跳过，并添加失败记录
                // 2. 默认构造的请求次数为 0， 并且不可用户更改，因此可以保证数据安全性
                if (request.RequestedTimes > Options.RetriedTimes)
                {
                    Logger.LogInformation($"The number of requests reached the limit: {request}");
                    continue;
                }

                request.Owner = Id;

                list.Add(request);
            }

            await _services.Scheduler.EnqueueAsync(list);

            return list.Count;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.LogInformation($"Initialize {Id}");
                await InitializeDataFlowsAsync();
                await RegisterConsumerAsync(stoppingToken);
                await RunAsync(stoppingToken);
                Logger.LogInformation($"{Id} started");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "spider execute error.");
            }
        }

        private async Task RegisterConsumerAsync(CancellationToken stoppingToken)
        {
            var topic = string.Format(Const.Topic.Spider, Id.ToUpper());

            Logger.LogInformation($"{Id} register topic {topic}");
            _consumer = new MessageQueue.AsyncMessageConsumer<byte[]>(topic);
            _consumer.Received += async bytes =>
            {
                var message = await GetMessageAsync(bytes);
                if (message == null)
                {
                    return;
                }

                if (message is Response response)
                {
                    // 1. 从请求队列中去除请求
                    var request = _requestedQueue.Dequeue(response.RequestHash);

                    if (request == null)
                    {
                        Logger.LogWarning($"{Id} dequeue {response.RequestHash} failed");
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            request.Agent = response.Agent;

                            await HandleResponseAsync(request, response, bytes);
                        }
                        else
                        {
                            Logger.LogError(
                                $"{Id} download {request.RequestUri}, {request.Hash} status code: {response.StatusCode} failed: {response.ReasonPhrase}");
                            // 每次调用添加会导致 Requested + 1, 因此失败多次的请求最终会被过滤不再加到调度队列
                            await AddRequestsAsync(request);

                            OnError?.Invoke(request, response);
                        }
                    }
                }
                else
                {
                    Logger.LogError($"{Id} receive error message {JsonConvert.SerializeObject(message)}");
                }
            };

            await _services.MessageQueue.ConsumeAsync(_consumer, stoppingToken);
        }

        private async Task<object> GetMessageAsync(byte[] bytes)
        {
            try
            {
                return await bytes.DeserializeAsync();
            }
            catch (Exception e)
            {
                Logger.LogError($"Deserialize message failed: {e}");
                return null;
            }
        }

        private async Task HandleResponseAsync(Request request, Response response, byte[] messageBytes)
        {
            DataFlowContext context = null;
            try
            {
                using var scope = _services.ServiceProvider.CreateScope();
                context = new DataFlowContext(scope.ServiceProvider, Options, request, response)
                {
                    MessageBytes = messageBytes,
                    Id = Id,
                    Name = Name
                };

                foreach (var dataFlow in _dataFlows)
                {
                    await dataFlow.HandleAsync(context);
                }
            }
            catch (Exception e)
            {
                // if download correct content, parser or storage failed by network or something else
                // retry it until trigger retryTimes limitation
                await AddRequestsAsync(request);
                Logger.LogError($"{Id} handle {JsonConvert.SerializeObject(request)} failed: {e}");
            }
            finally
            {
                ObjectUtilities.DisposeSafely(context);
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                var bucket = CreateBucket(Options.Speed);
                var batch = (int)Options.Batch;

                while (!stoppingToken.IsCancellationRequested)
                {
                    var requests = (await _services.Scheduler.DequeueAsync(batch)).ToArray();

                    if (requests.Length > 0)
                    {
                        foreach (var request in requests)
                        {
                            while (bucket.ShouldThrottle(1, out var waitTimeMillis))
                            {
                                await Task.Delay(waitTimeMillis, default);
                            }

                            await PublishRequestMessagesAsync(request);
                        }
                    }
                    else
                    {
                        OnSchedulerEmpty?.Invoke();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{Id} exited by exception: {e}");
            }
            finally
            {
                Stop();
            }
        }

        private static FixedTokenBucket CreateBucket(double speed)
        {
            if (speed >= 1)
            {
                var defaultTimeUnit = (int)(1000 / speed);
                return new FixedTokenBucket(1, 1, defaultTimeUnit);
            }
            else
            {
                var defaultTimeUnit = (int)((1 / speed) * 1000);
                return new FixedTokenBucket(1, 1, defaultTimeUnit);
            }
        }

        private async Task<bool> PublishRequestMessagesAsync(params Request[] requests)
        {
            if (requests.Length > 0)
            {
                foreach (var request in requests)
                {
                    string topic = string.IsNullOrEmpty(request.Downloader)
                            ? Const.Downloader.HttpClient
                            : request.Downloader;
                    request.Timestamp = DateTimeHelper.ToTimestamp(DateTimeOffset.Now);

                    if (_requestedQueue.Enqueue(request))
                    {
                        await _services.MessageQueue.PublishAsBytesAsync(topic, request);
                    }
                    else
                    {
                        Logger.LogWarning($"{Id} enqueue request: {request.RequestUri}, {request.Hash} failed");
                    }
                }
            }

            return true;
        }

        private async Task InitializeDataFlowsAsync()
        {
            if (_dataFlows.Count == 0)
            {
                Logger.LogWarning("{Id} there is no any dataFlow");
            }
            else
            {
                var dataFlowInfo = string.Join(" -> ", _dataFlows.Select(x => x.GetType().Name));
                Logger.LogInformation($"{Id} DataFlows: {dataFlowInfo}");
                foreach (var dataFlow in _dataFlows)
                {
                    dataFlow.SetLogger(Logger);
                    try
                    {
                        await dataFlow.InitAsync();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"{Id} initialize dataFlow {dataFlow.GetType().Name} failed: {e}");
                        throw e;
                    }
                }
            }
        }

        public void Dispose()
        {
            ObjectUtilities.DisposeSafely(Logger, _dataFlows);
            ObjectUtilities.DisposeSafely(Logger, _services);
        }
    }
}
