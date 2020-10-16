using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NETCore.LittleSpider.Agent;
using NETCore.LittleSpider.Downloader;
using NETCore.LittleSpider.Infrastructure;
using NETCore.LittleSpider.MessageQueue;
using NETCore.LittleSpider.Scheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLittleSpider(this IServiceCollection services,
            Action<SpiderOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddAgent<HttpClientDownloader>(options=>
            {
                options.AgentId = Guid.NewGuid().ToString("N");
            });
            services.TryAddSingleton<DependenceServices>();
            services.AddScheduler();
            services.AddMessageQueue();
            services.AddSingleton<IHashAlgorithmService, MD5HashAlgorithmService>();
            services.AddTransient<SpiderFactory>();
            return services;
        }
    }
}
