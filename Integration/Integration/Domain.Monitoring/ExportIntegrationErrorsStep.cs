using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
	public class ExportIntegrationErrorsStep : Step<MonitorWorkItem>
	{
		internal const string MessageGroupingPattern = "ErrorID: .+$";

		private readonly IDbFactory _db;

		private readonly ITaskFactory _taskFactory;

		public override string Description
		{
			get
			{
				return "Exports errors from integration error log.";
			}
		}

		public ExportIntegrationErrorsStep(IDbFactory db, ITaskFactory taskFactory)
		{
			this._db = db;
			this._taskFactory = taskFactory;
		}

		public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
		{
			workItem.AddMessageGroupingPatterns(new string[] { "ErrorID: .+$" });
			using (IDbSession dbSession = this._db.OpenSession())
			{
				ExportIntegrationErrorsStep.ErrorEntry[] array = dbSession.Query<ExportIntegrationErrorsStep.ErrorEntry>("\r\nSELECT\r\n\tErrorLog.Id AS ErrorId,\r\n\tErrorLog.[Message] AS ErrorMessage,\r\n\tErrorLog.[TimeStamp] AS [DateTime],\r\n\tErrorLog.Severity,\r\n\tErrorLog.[Target],\r\n\tTaskLog.TaskName,\r\n\tTaskLog.StepName\r\nFROM\r\n\tErrorLog\r\n\tOUTER APPLY (\r\n\t\tSELECT TOP 1 TaskLog.TaskName, TaskLog.StepName\r\n\t\tFROM TaskLog\r\n\t\tWHERE (TaskLog.ErrorLog_Id = ErrorLog.Id)\r\n\t\tORDER BY ID DESC\r\n\t) AS TaskLog\r\nWHERE (\r\n\tErrorLog.[TimeStamp] BETWEEN @LowerBound AND @UpperBound\r\n)\r\nORDER BY ErrorLog.Id DESC", new { LowerBound = workItem.CheckRange.LowerBound, UpperBound = workItem.CheckRange.UpperBound }).ToArray<ExportIntegrationErrorsStep.ErrorEntry>();
				if (array.Length != 0)
				{
					context.Log.Message("{0} entries within time-period {1}.", new object[] { (int)array.Length, workItem.CheckRange });
					Dictionary<string, ITask> strs = new Dictionary<string, ITask>(StringComparer.OrdinalIgnoreCase);
					ExportIntegrationErrorsStep.ErrorEntry[] errorEntryArray = array;
					for (int i = 0; i < (int)errorEntryArray.Length; i++)
					{
						ExportIntegrationErrorsStep.ErrorEntry description = errorEntryArray[i];
						string str = description.SafeTaskName();
						ITask task = null;
						if (!string.IsNullOrWhiteSpace(str) && !strs.TryGetValue(str, out task))
						{
							try
							{
								task = this._taskFactory.Get(str);
								strs.Add(str, task);
							}
							catch (TaskNotFoundException taskNotFoundException)
							{
							}
						}
						if (task != null)
						{
							description.TaskDescription = task.Description;
							IStep step = task.Steps.EmptyIfNull<IStep>().SingleOrDefault<IStep>((IStep x) => string.Equals(x.Name(), description.StepName, StringComparison.OrdinalIgnoreCase));
							if (step != null)
							{
								description.StepDescription = step.Description;
							}
						}
						workItem.Add(description.DateTime, "Integration Service", description.CombineMessage(), new Target[] { description.Target });
					}
				}
			}
		}

		public class ErrorEntry
		{
			public DateTimeOffset DateTime
			{
				get;
				set;
			}

			public int ErrorId
			{
				get;
				set;
			}

			public string ErrorMessage
			{
				get;
				set;
			}

			public Severity Severity
			{
				get;
				set;
			}

			public string StepDescription
			{
				get;
				set;
			}

			public string StepName
			{
				get;
				set;
			}

			public Target Target
			{
				get;
				set;
			}

			public string TaskDescription
			{
				get;
				set;
			}

			public string TaskName
			{
				get;
				set;
			}

			public ErrorEntry()
			{
			}

			public string CombineMessage()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("{0}:", this.Severity);
				stringBuilder.AppendLine();
				string str = this.SafeTaskName();
				if (!string.IsNullOrWhiteSpace(str))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("Task '{0}'", str);
					if (!string.IsNullOrWhiteSpace(this.TaskDescription))
					{
						stringBuilder.AppendFormat(": {0}", this.TaskDescription);
						stringBuilder.AppendLine();
					}
					if (!string.IsNullOrWhiteSpace(this.StepName))
					{
						stringBuilder.AppendFormat("Step '{0}'", this.StepName);
						if (!string.IsNullOrWhiteSpace(this.StepDescription))
						{
							stringBuilder.AppendFormat(": {0}", this.StepDescription);
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(this.ErrorMessage);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("ErrorID: {0}", this.ErrorId);
				return stringBuilder.ToString();
			}

			public string SafeTaskName()
			{
				return this.TaskName ?? string.Empty;
			}
		}
	}
}