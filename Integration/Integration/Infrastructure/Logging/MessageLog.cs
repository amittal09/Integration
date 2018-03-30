using System;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class MessageLog : LogEntry
	{
		public string Message
		{
			get;
			private set;
		}

		public Vertica.Integration.Infrastructure.Logging.StepLog StepLog
		{
			get;
		}

		public Vertica.Integration.Infrastructure.Logging.TaskLog TaskLog
		{
			get;
		}

		private MessageLog(Vertica.Integration.Infrastructure.Logging.TaskLog taskLog, string message) : base(false)
		{
			if (taskLog == null)
			{
				throw new ArgumentNullException("taskLog");
			}
			this.TaskLog = taskLog;
			this.Message = message.MaxLength(4000);
		}

		internal MessageLog(Vertica.Integration.Infrastructure.Logging.TaskLog taskLog, string message, Output output) : this(taskLog, message)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			output.Message("{0}: {1}", new object[] { taskLog.Name, message });
		}

		internal MessageLog(Vertica.Integration.Infrastructure.Logging.StepLog stepLog, string message, Output output) : this(stepLog.TaskLog, message)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			this.StepLog = stepLog;
			output.Message("{0}: {1}", new object[] { stepLog.Name, message });
		}

		public override void Dispose()
		{
			base.Dispose();
			this.TaskLog.Persist(this);
		}

		public override string ToString()
		{
			object stepLog = this.StepLog;
			if (stepLog == null)
			{
				stepLog = this.TaskLog;
			}
			return stepLog.ToString();
		}
	}
}