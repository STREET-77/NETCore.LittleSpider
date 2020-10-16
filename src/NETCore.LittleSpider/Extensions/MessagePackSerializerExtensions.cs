﻿using MessagePack;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NETCore.LittleSpider.Extensions
{
    public static class MessagePackSerializerExtensions
	{
		private static readonly MessagePackSerializerOptions SerializerOptions =
			MessagePackSerializer.Typeless.DefaultOptions.WithCompression(MessagePackCompression.Lz4Block);

		public static byte[] Serialize(this object message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var bytes = MessagePackSerializer.Typeless.Serialize(message, SerializerOptions);
			return bytes;
		}

		public static async Task<object> DeserializeAsync(this byte[] bytes,
			CancellationToken cancellationToken = default)
		{
			var stream = new MemoryStream(bytes);
			return await MessagePackSerializer.Typeless.DeserializeAsync(stream, SerializerOptions, cancellationToken);
		}

		public static async Task<T> DeserializeAsync<T>(this byte[] bytes,
			CancellationToken cancellationToken = default)
			where T : class
		{
			var result = await bytes.DeserializeAsync(cancellationToken);
			return result as T;
		}
	}
}
