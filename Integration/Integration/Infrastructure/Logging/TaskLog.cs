using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class TaskLog : LogEntry, IReferenceErrorLog
	{
		private readonly Action<LogEntry> _persist;

		private readonly Output _output;

		private readonly IList<StepLog> _steps;

		private readonly IList<MessageLog> _messages;

		public string CommandLine
		{
			get;
			private set;
		}

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

		public string IdentityName
		{
			get;
			private set;
		}

		public string MachineName
		{
			get;
			private set;
		}

		public ReadOnlyCollection<MessageLog> Messages
		{
			get
			{
				if (!this._messages.EmptyIfNull<MessageLog>().Any<MessageLog>())
				{
					return new ReadOnlyCollection<MessageLog>(new List<MessageLog>());
				}
				return new ReadOnlyCollection<MessageLog>(this._messages);
			}
		}

		public string Name
		{
			get;
		}

		public ReadOnlyCollection<StepLog> Steps
		{
			get
			{
				if (!this._steps.EmptyIfNull<StepLog>().Any<StepLog>())
				{
					return new ReadOnlyCollection<StepLog>(new List<StepLog>());
				}
				return new ReadOnlyCollection<StepLog>(this._steps);
			}
		}

		internal TaskLog(ITask task, Action<LogEntry> persist, Output output) : base(true)
		{
			if (persist == null)
			{
				throw new ArgumentNullException("persist");
			}
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			this._persist = persist;
			this._output = output;
			this._steps = new List<StepLog>();
			this._messages = new List<MessageLog>();
			this.Name = task.Name();
			this.MachineName = Environment.MachineName;
			this.IdentityName = WindowsUtils.GetIdentityName();
			this.CommandLine = Environment.CommandLine.MaxLength(4000);
			this.Persist(this);
			this._output.Message(this.Name, new object[0]);
		}

		public override void Dispose()
		{
			base.Dispose();
			this.Persist(this);
		}

		public void LogMessage(string message)
		{
			using (MessageLog messageLog = new MessageLog(this, message, this._output))
			{
				this._messages.Add(messageLog);
			}
		}

		public StepLog LogStep(IStep step)
		{
			if (step == null)
			{
				throw new ArgumentNullException("step");
			}
			StepLog stepLog = new StepLog(this, step, this._output);
			this._steps.Add(stepLog);
			return stepLog;
		}

		protected internal void Persist(LogEntry logEntry)
		{
			if (logEntry == null)
			{
				throw new ArgumentNullException("logEntry");
			}
			this._persist(logEntry);
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}