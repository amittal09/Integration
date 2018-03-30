using Castle.Windsor;
using System;
using System.IO;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	public class TextWriterLoggerConfiguration : IInitializable<IWindsorContainer>
	{
		private bool _detailed;

		internal TextWriterLoggerConfiguration()
		{
		}

		public TextWriterLoggerConfiguration Detailed()
		{
			this._detailed = true;
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance<TextWriterLoggerConfiguration>(this);
		}

		internal void Write(TextWriter textWriter, ErrorLog log)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException("textWriter");
			}
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			if (!this._detailed)
			{
				textWriter.WriteLine(log.FormattedMessage);
				return;
			}
			textWriter.WriteLine(string.Join(Environment.NewLine, new object[] { log.MachineName, log.IdentityName, log.CommandLine, log.Severity, log.Target, log.TimeStamp, string.Empty, "---- BEGIN LOG", string.Empty, log.Message, string.Empty, log.FormattedMessage }));
		}
	}
}