using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
	public static class MonitorTaskExtensions
	{
		public static TasksConfiguration MonitorTask(this TasksConfiguration configuration, Action<TaskConfiguration<MonitorWorkItem>> task = null)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			TaskConfiguration<MonitorWorkItem> taskConfiguration = null;
			configuration = configuration.Task<MonitorTask, MonitorWorkItem>((TaskConfiguration<MonitorWorkItem> x) => taskConfiguration = x);
			taskConfiguration.Step<ExportIntegrationErrorsStep>().Step<MonitorFoldersStep>().Step<PingUrlsStep>();
			if (task != null)
			{
				task(taskConfiguration);
			}
			return configuration;
		}
	}
}