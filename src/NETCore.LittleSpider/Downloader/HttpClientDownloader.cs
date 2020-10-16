using Microsoft.Extensions.Logging;
using NETCore.LittleSpider.Http;
using NETCore.LittleSpider.Infrastructure;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Downloader
{
    public class HttpClientDownloader : IDownloader
    {
        protected IHttpClientFactory HttpClientFactory { get; }
        protected ILogger Logger { get; }

        public HttpClientDownloader(IHttpClientFactory httpClientFactory,
            ILogger<HttpClientDownloader> logger)
        {
            HttpClientFactory = httpClientFactory;
            Logger = logger;
        }

        public async Task<Response> DownloadAsync(Request request)
        {
            HttpResponseMessage httpResponseMessage = null;
            HttpRequestMessage httpRequestMessage = null;
            try
            {
                httpRequestMessage = request.ToHttpRequestMessage();

                var httpClient = CreateClient(request);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                httpResponseMessage = await SendAsync(httpClient, httpRequestMessage);

                stopwatch.Stop();

                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                var response = await HandleAsync(request, httpResponseMessage);
                if (response != null)
                {
                    return response;
                }

                response = await httpResponseMessage.ToResponseAsync();
                response.ElapsedMilliseconds = (int)elapsedMilliseconds;
                response.RequestHash = request.Hash;

                return response;
            }
            catch (Exception e)
            {
                Logger.LogError($"{request.RequestUri} download failed: {e}");
                return new Response
                {
                    RequestHash = request.Hash,
                    StatusCode = HttpStatusCode.Gone,
                    ReasonPhrase = e.ToString()
                };
            }
            finally
            {
                ObjectUtilities.DisposeSafely(Logger, httpResponseMessage, httpRequestMessage);
            }
        }

        protected virtual async Task<HttpResponseMessage> SendAsync(HttpClient httpClient,
            HttpRequestMessage httpRequestMessage)
        {
            return await httpClient.SendAsync(httpRequestMessage);
        }

        protected virtual HttpClient CreateClient(Request request)
        {
            return HttpClientFactory.CreateClient(request.RequestUri.Host);
        }

        protected virtual Task<Response> HandleAsync(Request request, HttpResponseMessage responseMessage)
        {
            return Task.FromResult((Response)null);
        }

        public virtual string Name => Const.Downloader.HttpClient;
    }
}
