using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETCore.LittleSpider.Extensions
{
	public static class EnumerableExtensions
	{
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
		{
			return list == null || !list.Any();
		}
	}
}
