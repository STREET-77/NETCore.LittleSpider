using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NETCore.LittleSpider.MessageQueue;
using NETCore.LittleSpider.Scheduler;
using System;

namespace NETCore.LittleSpider
{
    public class DependenceServices : IDisposable
	{
		public IServiceProvider ServiceProvider { get; }
		public IScheduler Scheduler { get; }
		public IMessageQueue MessageQueue { get; }
		public HostBuilderContext HostBuilderContext { get; }

		public IConfiguration Configuration { get; }

		public DependenceServices(IServiceProvider serviceProvider,
			IScheduler scheduler,
			IMessageQueue messageQueue,
			IConfiguration configuration,
			HostBuilderContext builderContext)
		{
			ServiceProvider = serviceProvider;
			Scheduler = scheduler;
			MessageQueue = messageQueue;
			HostBuilderContext = builderContext;
			Configuration = configuration;
		}

		public void Dispose()
		{
			MessageQueue.Dispose();
			Scheduler.Dispose();
		}
	}
}
