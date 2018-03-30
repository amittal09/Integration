using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Logging
{
	public abstract class LogEntry : IDisposable
	{
		private readonly Stopwatch _watch;

		public double? ExecutionTimeSeconds
		{
			get;
			private set;
		}

		public string Id
		{
			get;
			internal set;
		}

		public DateTimeOffset TimeStamp
		{
			get;
			private set;
		}

		protected LogEntry(bool measureExecutionTime = true)
		{
			this._watch = new Stopwatch();
			if (measureExecutionTime)
			{
				this._watch.Start();
			}
			this.TimeStamp = Time.UtcNow;
		}

		public virtual void Dispose()
		{
			if (this._watch.IsRunning)
			{
				this._watch.Stop();
				this.ExecutionTimeSeconds = new double?(this._watch.Elapsed.TotalSeconds);
			}
		}
	}
}