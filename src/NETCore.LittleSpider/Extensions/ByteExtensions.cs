using System;
using System.Collections.Generic;
using System.Text;

namespace NETCore.LittleSpider.Extensions
{
	public static class ByteExtensions
	{
		public static string ToBase64String(this byte[] bytes)
		{
			var builder = new StringBuilder();
			foreach (var b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}
	}
}
