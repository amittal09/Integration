using System;

namespace Vertica.Integration.Model
{
	internal class Output
	{
		private readonly Action<string> _message;

		public Output(Action<string> message)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
			this._message = message;
		}

		public void Message(string format, params object[] args)
		{
			this._message(string.Format(format, args));
		}
	}
}