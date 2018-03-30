using System;

namespace Vertica.Integration.Infrastructure.Logging
{
	public interface ILogger
	{
		IDisposable Disable();

		void LogEntry(LogEntry entry);

		ErrorLog LogError(ITarget target, string message, params object[] args);

		ErrorLog LogError(Exception exception, ITarget target = null);

		ErrorLog LogWarning(ITarget target, string message, params object[] args);
	}
}