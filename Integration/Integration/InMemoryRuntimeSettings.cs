using System;
using System.Collections.Generic;
using System.Reflection;

namespace Vertica.Integration
{
	public class InMemoryRuntimeSettings : IRuntimeSettings
	{
		private readonly IDictionary<string, string> _values;

		public ApplicationEnvironment Environment
		{
			get
			{
				return this["Environment"];
			}
		}

		public string this[string name]
		{
			get
			{
				string str;
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", "name");
				}
				this._values.TryGetValue(name, out str);
				return str;
			}
		}

		public InMemoryRuntimeSettings(IDictionary<string, string> values = null)
		{
			object strs = values;
			if (strs == null)
			{
				strs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
			this._values = (IDictionary<string, string>)strs;
		}

		public InMemoryRuntimeSettings Set(string name, string value)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this._values[name] = value;
			return this;
		}
	}
}