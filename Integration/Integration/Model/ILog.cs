using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
	public interface ILog
	{
		ErrorLog Error(ITarget target, string format, params object[] args);

		ErrorLog Exception(Exception exception, ITarget target = null);

		void Message(string format, params object[] args);

		ErrorLog Warning(ITarget target, string format, params object[] args);
	}
}