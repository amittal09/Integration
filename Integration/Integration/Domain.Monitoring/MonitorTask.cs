using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorTask : Task<MonitorWorkItem>
	{
		private readonly IConfigurationService _configuration;

		private readonly IEmailService _emailService;

		private readonly IRuntimeSettings _runtimeSettings;

		public override string Description
		{
			get
			{
				return "Monitors the solution and sends out e-mails if there is any errors.";
			}
		}

		public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IConfigurationService configuration, IEmailService emailService, IRuntimeSettings runtimeSettings) : base(steps)
		{
			this._configuration = configuration;
			this._emailService = emailService;
			this._runtimeSettings = runtimeSettings;
		}

		public override void End(MonitorWorkItem workItem, ITaskExecutionContext context)
		{
			Target[] targetArray;
			if (workItem.HasEntriesForUnconfiguredTargets(out targetArray))
			{
				context.Log.Error(Target.Service, "Create missing configuration for the following targets: [{0}].", new object[] 
                { string.Join(", ", from x in (IEnumerable<Target>)targetArray select x.Name) });
			}
			MonitorTarget[] targets = workItem.Configuration.Targets ?? new MonitorTarget[0];
			for (int i = 0; i < (int)targets.Length; i++)
			{
				MonitorTarget monitorTarget = targets[i];
				this.SendTo(monitorTarget, workItem, context.Log, workItem.Configuration.SubjectPrefix);
			}
			workItem.Configuration.LastRun = workItem.CheckRange.UpperBound;
			this._configuration.Save<MonitorConfiguration>(workItem.Configuration, base.Name, false);
		}

		private void SendTo(MonitorTarget target, MonitorWorkItem workItem, ILog log, string subjectPrefix)
		{
			MonitorEntry[] entries = workItem.GetEntries(target);
			if (entries.Length != 0)
			{
				if (target.Recipients == null || target.Recipients.Length == 0)
				{
					log.Warning(Target.Service, "No recipients found for target '{0}'.", new object[] { target });
					return;
				}
				log.Message("Sending {0} entries to {1}.", new object[] { (int)entries.Length, target });
				StringBuilder stringBuilder = new StringBuilder();
				ApplicationEnvironment environment = this._runtimeSettings.Environment;
				if (environment != null)
				{
					stringBuilder.AppendFormat("[{0}] ", environment);
				}
				if (!string.IsNullOrWhiteSpace(subjectPrefix))
				{
					stringBuilder.AppendFormat("{0}: ", subjectPrefix);
				}
				stringBuilder.AppendFormat("Monitoring ({0})", workItem.CheckRange);
				this._emailService.Send(new MonitorEmailTemplate(stringBuilder.ToString(), entries, target), target.Recipients);
			}
		}

		public override MonitorWorkItem Start(ITaskExecutionContext context)
		{
			MonitorConfiguration monitorConfiguration = this._configuration.Get<MonitorConfiguration>();
			monitorConfiguration.Assert();
			return (new MonitorWorkItem(monitorConfiguration)).AddIgnoreFilter(new MessageContainsText(monitorConfiguration.IgnoreErrorsWithMessagesContaining)).AddTargetRedirect(new RedirectForMonitorTargets(monitorConfiguration.Targets)).AddMessageGroupingPatterns(monitorConfiguration.MessageGroupingPatterns);
		}
	}
}