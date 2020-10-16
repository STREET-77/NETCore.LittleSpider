﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.MessageQueue
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMessageQueue(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IMessageQueue, MessageQueue>();
			return serviceCollection;
		}
	}
}
