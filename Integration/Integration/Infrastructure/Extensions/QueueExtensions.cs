using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Extensions
{
	internal static class QueueExtensions
	{
		public static T SafePeek<T>(this Queue<T> queue, T valueIfEmpty)
		{
			if (queue == null)
			{
				throw new ArgumentNullException("queue");
			}
			if (queue.Count <= 0)
			{
				return valueIfEmpty;
			}
			return queue.Peek();
		}
	}
}