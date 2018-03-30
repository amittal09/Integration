using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration
{
	public class ConfigurationBasedRuntimeSettings : IRuntimeSettings
	{
		private readonly IConfigurationService _configuration;

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
				this._configuration.Get<ConfigurationBasedRuntimeSettings.RuntimeSettings>().Values.TryGetValue(name, out str);
				return str;
			}
		}

		public ConfigurationBasedRuntimeSettings(IConfigurationService configuration)
		{
			this._configuration = configuration;
		}

		[Description("General purpose configuration used by various tasks, services etc.")]
		[Guid("8560D663-8E0D-4E27-84D4-9561CA35ED2A")]
		public class RuntimeSettings
		{
			internal const string Environment = "Environment";

			public Dictionary<string, string> Values
			{
				get;
				set;
			}

			public RuntimeSettings()
			{
				this.Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				this.Values["Environment"] = string.Empty;
			}
		}
	}
}