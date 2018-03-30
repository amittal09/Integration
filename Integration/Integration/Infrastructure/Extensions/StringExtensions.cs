using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Extensions
{
	internal static class StringExtensions
	{
		public static string MaxLength(this string value, uint maxLength)
		{
			if (value == null || (ulong)value.Length <= (ulong)maxLength)
			{
				return value;
			}
			return value.Substring(0, (int)maxLength);
		}
	}
}