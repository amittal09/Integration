using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorEntry
	{
		public DateTimeOffset DateTime
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public string Source
		{
			get;
			private set;
		}

		public MonitorEntry(DateTimeOffset dateTime, string source, string message)
		{
			this.DateTime = dateTime;
			this.Source = source;
			this.Message = message;
		}
	}
}