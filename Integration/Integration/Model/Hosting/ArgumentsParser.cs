using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Hosting
{
	public class ArgumentsParser : IArgumentsParser
	{
		public ArgumentsParser()
		{
		}

		private static KeyValuePair<string, string> Map(string key, Queue<string> remaining)
		{
			string str;
			string str1 = null;
			int num = key.IndexOf(":", StringComparison.InvariantCulture);
			if (num > 0)
			{
				str1 = key.Substring(num + 1);
				key = key.Substring(0, num);
			}
			else if (remaining.SafePeek<string>(string.Empty).StartsWith(":"))
			{
				str1 = remaining.Dequeue().Substring(1);
			}
			if (str1 != null && str1.Length == 0)
			{
				if (remaining.Count > 0)
				{
					str = remaining.Dequeue();
				}
				else
				{
					str = null;
				}
				str1 = str;
			}
			return new KeyValuePair<string, string>(key, str1);
		}

		public HostArguments Parse(string[] arguments)
		{
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			string empty = arguments.FirstOrDefault<string>() ?? string.Empty;
			if (empty.TrimStart(new char[0]).StartsWith("-"))
			{
				empty = string.Empty;
			}
			List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();
			List<KeyValuePair<string, string>> keyValuePairs1 = new List<KeyValuePair<string, string>>();
			Queue<string> strs = new Queue<string>(arguments.Skip<string>((string.IsNullOrWhiteSpace(empty) ? 0 : 1)));
			while (strs.Count > 0)
			{
				string str = strs.Dequeue();
				if (!str.StartsWith("-"))
				{
					keyValuePairs1.Add(ArgumentsParser.Map(str, strs));
				}
				else
				{
					if (str.Length <= 1)
					{
						continue;
					}
					keyValuePairs.Add(ArgumentsParser.Map(str.Substring(1), strs));
				}
			}
			return new HostArguments(empty, keyValuePairs.ToArray(), keyValuePairs1.ToArray());
		}
	}
}