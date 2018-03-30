using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	public abstract class Logger : ILogger
	{
		private readonly object _dummy = new object();

		private readonly Stack<object> _disablers;

		private readonly ChainOfResponsibilityLink<LogEntry> _handlers;

		private bool LoggingDisabled
		{
			get
			{
				return this._disablers.Count > 0;
			}
		}

		protected Logger()
		{
			this._disablers = new Stack<object>();
			ChainOfResponsibilityLink<LogEntry> chainOfResponsibilityLink = ChainOfResponsibility.Empty<LogEntry>();
			Logger logger = this;
			Logger logger1 = this;
			ChainOfResponsibilityLink<LogEntry> chainOfResponsibilityLink1 = chainOfResponsibilityLink.Chain(new Logger.LogEntryLink<TaskLog>(new Func<TaskLog, string>(logger.Insert), new Action<TaskLog>(logger1.Update)));
			Logger logger2 = this;
			Logger logger3 = this;
			Logger logger4 = this;
			this._handlers = chainOfResponsibilityLink1.Chain(new Logger.LogEntryLink<StepLog>(new Func<StepLog, string>(logger2.Insert), new Action<StepLog>(logger3.Update))).Chain(new Logger.LogEntryLink<MessageLog>(new Func<MessageLog, string>(logger4.Insert), null));
		}

		public IDisposable Disable()
		{
			this._disablers.Push(this._dummy);
			return new Logger.Disabler(() => this._disablers.Pop());
		}

		protected abstract string Insert(TaskLog log);

		protected abstract string Insert(MessageLog log);

		protected abstract string Insert(StepLog log);

		protected abstract string Insert(ErrorLog log);

		public void LogEntry(LogEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (this.LoggingDisabled)
			{
				return;
			}
			this._handlers.Handle(entry);
		}

		public ErrorLog LogError(ITarget target, string message, params object[] args)
		{
			return this.LogError(new ErrorLog(Severity.Error, string.Format(message, args), target));
		}

		public ErrorLog LogError(Exception exception, ITarget target = null)
		{
			return this.LogError(new ErrorLog(exception, target));
		}

		private ErrorLog LogError(ErrorLog errorLog)
		{
			if (this.LoggingDisabled)
			{
				return null;
			}
			errorLog.Id = this.Insert(errorLog);
			return errorLog;
		}

		public ErrorLog LogWarning(ITarget target, string message, params object[] args)
		{
			return this.LogError(new ErrorLog(Severity.Warning, string.Format(message, args), target));
		}

		protected abstract void Update(TaskLog log);

		protected abstract void Update(StepLog log);

		private class Disabler : IDisposable
		{
			private readonly Action _disposed;

			private bool _wasDisposed;

			public Disabler(Action disposed)
			{
				this._disposed = disposed;
			}

			public void Dispose()
			{
				if (!this._wasDisposed)
				{
					this._disposed();
					this._wasDisposed = true;
				}
			}
		}

		private class LogEntryLink<TLogEntry> : IChainOfResponsibilityLink<LogEntry>
		where TLogEntry : LogEntry
		{
			private readonly Func<TLogEntry, string> _insert;

			private readonly Action<TLogEntry> _update;

			public LogEntryLink(Func<TLogEntry, string> insert, Action<TLogEntry> update = null)
			{
				if (insert == null)
				{
					throw new ArgumentNullException("insert");
				}
				this._insert = insert;
				this._update = update;
			}

			public bool CanHandle(LogEntry context)
			{
				return context is TLogEntry;
			}

			public void DoHandle(LogEntry context)
			{
				if (context.Id == null)
				{
					context.Id = this._insert((TLogEntry)(context as TLogEntry));
					return;
				}
				if (this._update == null)
				{
					throw new NotSupportedException(string.Format("Update for '{0}' is not supported.", typeof(TLogEntry).Name));
				}
				this._update((TLogEntry)(context as TLogEntry));
			}
		}
	}
}