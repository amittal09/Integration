using System;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class DefaultHandler : IProcessExitHandler
	{
		private readonly TextWriter _outputter;

		public DefaultHandler(TextWriter outputter)
		{
			this._outputter = outputter;
		}

		public void Wait()
		{
			this._outputter.WaitUntilEscapeKeyIsHit("Press ESCAPE to continue...");
		}
	}
}