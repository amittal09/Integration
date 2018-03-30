using System;
using System.IO;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class TextWriterLogger : Logger
	{
		private readonly TextWriter _textWriter;

		private readonly TextWriterLoggerConfiguration _configuration;

		public TextWriterLogger(TextWriter textWriter, TextWriterLoggerConfiguration configuration)
		{
			this._textWriter = textWriter;
			this._configuration = configuration;
		}

		protected override string Insert(TaskLog log)
		{
			return null;
		}

		protected override string Insert(StepLog log)
		{
			return null;
		}

		protected override string Insert(MessageLog log)
		{
			return null;
		}

		protected override string Insert(ErrorLog log)
		{
			this._configuration.Write(this._textWriter, log);
			return null;
		}

		protected override void Update(TaskLog log)
		{
		}

		protected override void Update(StepLog log)
		{
		}
	}
}