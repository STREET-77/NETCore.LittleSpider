using NETCore.LittleSpider.MessageQueue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Extensions
{
	public static class MessageQueueExtensions
	{
		public static async Task PublishAsBytesAsync<T>(this IMessageQueue messageQueue, string queue, T message)
		{
			var bytes = message.Serialize();
			await messageQueue.PublishAsync(queue, bytes);
		}
	}
}
