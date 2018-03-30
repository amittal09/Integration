using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class MaintenanceWorkItem : ContextWorkItem
	{
		public MaintenanceConfiguration Configuration
		{
			get;
			private set;
		}

		public MaintenanceWorkItem(MaintenanceConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			this.Configuration = configuration;
		}
	}
}