using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
	internal class Log : ILog
	{
		private readonly ILogger _logger;

		private readonly Action<string> _message;

		internal Log(Action<string> message, ILogger logger)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
			if (logger == null)
			{
				throw new ArgumentNullException("logger");
			}
			this._message = message;
			this._logger = logger;
		}

		public ErrorLog Error(ITarget target, string format, params object[] args)
		{
			string str = format;
			if (args != null && args.Length != 0)
			{
				str = string.Format(str, args);
			}
			this._message(string.Format("[ERROR] {0}", str));
			return this._logger.LogError(target, str, new object[0]);
		}

		public ErrorLog Exception(Exception exception, ITarget target = null)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			this._message(string.Format("[ERROR] {0}", exception.Message));
			return this._logger.LogError(exception, target);
		}

		public void Message(string format, params object[] args)
		{
			if (args == null || args.Length == 0)
			{
				this._message(format);
				return;
			}
			this._message(string.Format(format, args));
		}

		public ErrorLog Warning(ITarget target, string format, params object[] args)
		{
			string str = format;
			if (args != null && args.Length != 0)
			{
				str = string.Format(str, args);
			}
			this._message(string.Format("[WARNING] {0}", str));
			return this._logger.LogWarning(target, str, new object[0]);
		}
	}
}