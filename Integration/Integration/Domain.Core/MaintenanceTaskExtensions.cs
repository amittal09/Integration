using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public static class MaintenanceTaskExtensions
	{
		public static TasksConfiguration MaintenanceTask(this TasksConfiguration configuration, Action<TaskConfiguration<MaintenanceWorkItem>> task = null)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			TaskConfiguration<MaintenanceWorkItem> taskConfiguration = null;
			configuration = configuration.Task<MaintenanceTask, MaintenanceWorkItem>((TaskConfiguration<MaintenanceWorkItem> x) => taskConfiguration = x);
			taskConfiguration.Step<CleanUpIntegrationDbStep>().Step<CleanUpArchivesStep>().Step<ArchiveFoldersStep>();
			if (task != null)
			{
				task(taskConfiguration);
			}
			return configuration;
		}
	}
}