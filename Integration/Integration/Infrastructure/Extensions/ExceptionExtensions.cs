using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vertica.Integration.Infrastructure.Extensions
{
	internal static class ExceptionExtensions
	{
		public static string AggregateMessages(this Exception exception)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int[] numArray = new int[1];
			Func<string, string> func = (string msg) => string.Concat(new string('-', numArray[0] * 3), " ", msg).Trim();
			while (exception != null)
			{
				stringBuilder.AppendLine(func(exception.GetType().FullName));
				stringBuilder.Append(func(exception.Message));
				exception = exception.InnerException;
				if (exception != null)
				{
					stringBuilder.AppendLine();
				}
				numArray[0]++;
			}
			return stringBuilder.ToString();
		}

		public static string GetFullStacktrace(this Exception exception)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (exception != null)
			{
				stringBuilder.AppendLine(exception.GetType().FullName);
				stringBuilder.AppendLine(exception.Message);
				stringBuilder.AppendLine();
				stringBuilder.Append(exception.StackTrace);
				foreach (DictionaryEntry datum in exception.Data)
				{
					stringBuilder.AppendFormat("{0} = {1}", datum.Key, datum.Value);
					stringBuilder.AppendLine();
				}
				exception = exception.InnerException;
				if (exception == null)
				{
					continue;
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}