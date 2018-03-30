using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class EventLogger : Logger
	{
		private readonly static CultureInfo English;

		private readonly string _sourceName;

		static EventLogger()
		{
			EventLogger.English = CultureInfo.GetCultureInfo("en-US");
		}

		public EventLogger(EventLoggerConfiguration configuration, IRuntimeSettings runtimeSettings)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			if (runtimeSettings == null)
			{
				throw new ArgumentNullException("runtimeSettings");
			}
			this._sourceName = configuration.SourceName ?? EventLogger.IntegrationService(runtimeSettings);
		}

		private ErrorLog CheckGetError(LogEntry log)
		{
			IReferenceErrorLog referenceErrorLog = log as IReferenceErrorLog;
			if (referenceErrorLog == null)
			{
				return null;
			}
			return referenceErrorLog.ErrorLog;
		}

		private string ErrorLine(ErrorLog error, string name)
		{
			return this.Line(error.TimeStamp, "[{0}] [{1}]: {2} (ID: {3})", new object[] { name, error.Severity, error.Message, error.Id });
		}

		private string ExecutionTime(LogEntry log)
		{
			double valueOrDefault = log.ExecutionTimeSeconds.GetValueOrDefault();
			return string.Format("(Execution time: {0} second(s))", valueOrDefault.ToString(EventLogger.English));
		}

		private static int GenerateEventId()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Environment.StackTrace);
			StackFrame[] frames = (new StackTrace()).GetFrames() ?? new StackFrame[0];
			for (int i = 0; i < (int)frames.Length; i++)
			{
				stringBuilder.Append(frames[i].GetILOffset());
				stringBuilder.Append(",");
			}
			return stringBuilder.ToString().GetHashCode() & 65535;
		}

		protected override string Insert(TaskLog log)
		{
			return EventLogger.GenerateEventId().ToString();
		}

		protected override string Insert(MessageLog log)
		{
			return null;
		}

		protected override string Insert(StepLog log)
		{
			return null;
		}

		protected override string Insert(ErrorLog log)
		{
			int num = EventLogger.GenerateEventId();
			string str = string.Join(Environment.NewLine, new object[] { log.MachineName, log.IdentityName, log.CommandLine, log.Severity, log.Target, log.TimeStamp, string.Empty, "---- BEGIN LOG", string.Empty, log.Message, string.Empty, log.FormattedMessage });
			EventLog.WriteEntry(this._sourceName, str, (log.Severity == Severity.Error ? EventLogEntryType.Error : EventLogEntryType.Warning), num);
			return num.ToString();
		}

		private static string IntegrationService(IRuntimeSettings runtimeSettings)
		{
			ApplicationEnvironment environment = runtimeSettings.Environment;
			return string.Concat("Integration Service", (environment != null ? string.Format(" [{0}]", environment) : string.Empty));
		}

		private string Line(LogEntry log, string text = null, params object[] args)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = string.Concat(" ", string.Format(text, args));
			}
			return this.Line(log.TimeStamp, string.Format("[{0}]{1}", log, text), new object[0]);
		}

		private string Line(DateTimeOffset timestamp, string text, params object[] args)
		{
			return string.Concat(Environment.NewLine, string.Format("[{0:HH:mm:ss}] {1}", timestamp.LocalDateTime, string.Format(text, args)));
		}

		protected override void Update(TaskLog log)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Join(Environment.NewLine, new string[] { log.MachineName, log.IdentityName, log.CommandLine, string.Empty, "---- BEGIN LOG" }));
			foreach (LogEntry logEntry in 
				from x in ((IEnumerable<LogEntry>)(new LogEntry[] { log })).Concat<LogEntry>(log.Messages).Concat<LogEntry>(log.Steps).Concat<LogEntry>(log.Steps.SelectMany<StepLog, MessageLog>((StepLog s) => s.Messages))
				orderby x.TimeStamp
				select x)
			{
				MessageLog messageLog = logEntry as MessageLog;
				stringBuilder.Append(this.Line(logEntry, (messageLog != null ? messageLog.Message : this.ExecutionTime(logEntry)), new object[0]));
				ErrorLog errorLog = this.CheckGetError(logEntry);
				if (errorLog == null)
				{
					continue;
				}
				stringBuilder.Append(this.ErrorLine(errorLog, logEntry.ToString()));
			}
			EventLog.WriteEntry(this._sourceName, stringBuilder.ToString(), EventLogEntryType.Information, int.Parse(log.Id));
		}

		protected override void Update(StepLog log)
		{
		}
	}
}