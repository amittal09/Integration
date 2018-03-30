using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

namespace Vertica.Integration
{
	public class AppConfigRuntimeSettings : IRuntimeSettings
	{
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
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", "name");
				}
				return ConfigurationManager.AppSettings[name];
			}
		}

		public AppConfigRuntimeSettings()
		{
		}
	}
}