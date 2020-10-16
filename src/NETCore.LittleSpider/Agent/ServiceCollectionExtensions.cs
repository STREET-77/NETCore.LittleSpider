using Microsoft.Extensions.DependencyInjection;
using NETCore.LittleSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.Agent
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgent<TDownloader>(this IServiceCollection services,
			Action<AgentOptions> configure = null)
			where TDownloader : class, IDownloader
		{
			services.AddHttpClient();

			if (configure != null)
			{
				services.Configure(configure);
			}

			services.AddSingleton<IDownloader, TDownloader>();
			services.AddHostedService<AgentService>();
			return services;
		}
	}
}
