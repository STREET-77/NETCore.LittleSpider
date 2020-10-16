using System;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.MessageQueue
{
    public interface IMessageQueue : IDisposable
	{
		Task PublishAsync(string queue, byte[] message);

		Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer, CancellationToken cancellationToken);

		void CloseQueue(string queue);
	}
}
