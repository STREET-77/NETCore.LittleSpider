using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.Scheduler
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddScheduler(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IScheduler, Impl.Scheduler>();
			return serviceCollection;
		}
	}
}
