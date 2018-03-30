using System;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class LoggingConfiguration
	{
		public ApplicationConfiguration Application
		{
			get;
		}

		internal LoggingConfiguration(ApplicationConfiguration application)
		{
			if (application == null)
			{
				throw new ArgumentNullException("application");
			}
			this.Application = application;
			this.Application.Extensibility((ExtensibilityConfiguration extensibility) => extensibility.Register<EventLoggerConfiguration>(() => new EventLoggerConfiguration()));
		}

		public LoggingConfiguration Disable()
		{
			return this.Use<VoidLogger>();
		}

		public LoggingConfiguration EventLogger(Action<EventLoggerConfiguration> eventLogger = null)
		{
			if (eventLogger != null)
			{
				this.Application.Extensibility((ExtensibilityConfiguration extensibility) => eventLogger(extensibility.Get<EventLoggerConfiguration>()));
			}
			return this.Use<EventLogger>();
		}

		public LoggingConfiguration TextFileLogger(Action<TextFileLoggerConfiguration> textFileLogger = null)
		{
			this.Application.Extensibility((ExtensibilityConfiguration extensibility) => {
				ExtensibilityConfiguration extensibilityConfigurations = extensibility;
				//Func<TextFileLoggerConfiguration> u003cu003e9_71 = LoggingConfiguration.<>c.<>9__7_1;
				//if (u003cu003e9_71 == null)
				//{
				//	u003cu003e9_71 = () => new TextFileLoggerConfiguration();
				////	LoggingConfiguration.<>c.<>9__7_1 = u003cu003e9_71;
				//}
				//TextFileLoggerConfiguration textFileLoggerConfiguration = extensibilityConfigurations.Register<TextFileLoggerConfiguration>(u003cu003e9_71);
				//if (textFileLogger != null)
				//{
				//	textFileLogger(textFileLoggerConfiguration);
				//}
			});
			return this.Use<TextFileLogger>();
		}

		public LoggingConfiguration TextWriter(Action<TextWriterLoggerConfiguration> textWriterLogger = null)
		{
			this.Application.Extensibility((ExtensibilityConfiguration extensibility) => {
				ExtensibilityConfiguration extensibilityConfigurations = extensibility;
				//Func<TextWriterLoggerConfiguration> u003cu003e9_81 = LoggingConfiguration.<>c.<>9__8_1;
				//if (u003cu003e9_81 == null)
				//{
				//	u003cu003e9_81 = () => new TextWriterLoggerConfiguration();
				//	LoggingConfiguration.<>c.<>9__8_1 = u003cu003e9_81;
				//}
				//TextWriterLoggerConfiguration textWriterLoggerConfiguration = extensibilityConfigurations.Register<TextWriterLoggerConfiguration>(u003cu003e9_81);
				//if (textWriterLogger != null)
				//{
				//	textWriterLogger(textWriterLoggerConfiguration);
				//}
			});
			return this.Use<TextWriterLogger>();
		}

		public LoggingConfiguration Use<T>()
		where T : Logger
		{
			this.Application.Advanced((AdvancedConfiguration advanced) => advanced.Register<ILogger, T>());
			return this;
		}
	}
}