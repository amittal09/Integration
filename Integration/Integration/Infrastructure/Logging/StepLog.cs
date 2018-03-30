using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class StepLog : LogEntry, IReferenceErrorLog
	{
		private readonly Output _output;

		private readonly IList<MessageLog> _messages;

		public Vertica.Integration.Infrastructure.Logging.ErrorLog ErrorLog
		{
			get
			{
				return JustDecompileGenerated_get_ErrorLog();
			}
			set
			{
				JustDecompileGenerated_set_ErrorLog(value);
			}
		}

		private Vertica.Integration.Infrastructure.Logging.ErrorLog JustDecompileGenerated_ErrorLog_k__BackingField;

		public Vertica.Integration.Infrastructure.Logging.ErrorLog JustDecompileGenerated_get_ErrorLog()
		{
			return this.JustDecompileGenerated_ErrorLog_k__BackingField;
		}

		internal void JustDecompileGenerated_set_ErrorLog(Vertica.Integration.Infrastructure.Logging.ErrorLog value)
		{
			this.JustDecompileGenerated_ErrorLog_k__BackingField = value;
		}

		public ReadOnlyCollection<MessageLog> Messages
		{
			get
			{
				return new ReadOnlyCollection<MessageLog>(this._messages);
			}
		}

		public string Name
		{
			get;
		}

		public Vertica.Integration.Infrastructure.Logging.TaskLog TaskLog
		{
			get;
		}

		internal StepLog(Vertica.Integration.Infrastructure.Logging.TaskLog taskLog, IStep step, Output output) : base(true)
		{
			if (taskLog == null)
			{
				throw new ArgumentNullException("taskLog");
			}
			if (step == null)
			{
				throw new ArgumentNullException("step");
			}
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			this._output = output;
			this._messages = new List<MessageLog>();
			this.TaskLog = taskLog;
			this.Name = step.Name();
			this.TaskLog.Persist(this);
			this._output.Message(this.Name, new object[0]);
		}

		public override void Dispose()
		{
			base.Dispose();
			this.TaskLog.Persist(this);
		}

		public void LogMessage(string message)
		{
			using (MessageLog messageLog = new MessageLog(this, message, this._output))
			{
				this._messages.Add(messageLog);
			}
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}