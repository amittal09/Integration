using System;
using System.Globalization;
using System.IO;
using System.Text;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class TextFileLogger : Logger
	{
		private readonly static CultureInfo English;

		private readonly string _baseDirectory;

		private readonly TextFileLoggerConfiguration _configuration;

		static TextFileLogger()
		{
			TextFileLogger.English = CultureInfo.GetCultureInfo("en-US");
		}

		public TextFileLogger(IRuntimeSettings settings, TextFileLoggerConfiguration configuration)
		{
			this._baseDirectory = settings["TextLogger.BaseDirectory"].NullIfEmpty() ?? "Data\\Logs";
			this._configuration = configuration;
		}

		private string EndLine(LogEntry log)
		{
			object[] str = new object[1];
			double valueOrDefault = log.ExecutionTimeSeconds.GetValueOrDefault();
			str[0] = valueOrDefault.ToString(TextFileLogger.English);
			return this.Line(log, "Execution time: {0} second(s)", str);
		}

		private FileInfo EnsureFilePath(TaskLog log)
		{
			return this.EnsureFilePath(this._configuration.GetFilePath(log, this._baseDirectory));
		}

		private FileInfo EnsureFilePath(ErrorLog log)
		{
			return this.EnsureFilePath(this._configuration.GetFilePath(log, this._baseDirectory));
		}

		private FileInfo EnsureFilePath(FileInfo filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			DirectoryInfo directory = filePath.Directory;
			if (directory == null)
			{
				throw new InvalidOperationException(string.Format("No directory specified for path '{0}'.", filePath.FullName));
			}
			if (!directory.Exists)
			{
				directory.Create();
			}
			return filePath;
		}

		private string ErrorLine(ErrorLog error, string name)
		{
			return this.Line(error.TimeStamp, "[{0}] [{1}]: {2} (ID: {3})", new object[] { name, error.Severity, error.Message, error.Id });
		}

		protected override string Insert(TaskLog log)
		{
			FileInfo fileInfo = this.EnsureFilePath(log);
			File.WriteAllText(fileInfo.FullName, string.Join(Environment.NewLine, new string[] { log.MachineName, log.IdentityName, log.CommandLine, string.Empty, "---- BEGIN LOG", this.Line(log, null, new object[0]) }));
			return fileInfo.Name;
		}

		protected override string Insert(StepLog log)
		{
			File.AppendAllText(this.EnsureFilePath(log.TaskLog).FullName, this.Line(log, null, new object[0]));
			return log.TaskLog.Id;
		}

		protected override string Insert(MessageLog log)
		{
			File.AppendAllText(this.EnsureFilePath(log.TaskLog).FullName, this.Line(log, "{0}", new object[] { log.Message }));
			return log.TaskLog.Id;
		}

		protected override string Insert(ErrorLog log)
		{
			FileInfo fileInfo = this.EnsureFilePath(log);
			File.WriteAllText(fileInfo.FullName, string.Join(Environment.NewLine, new object[] { log.MachineName, log.IdentityName, log.CommandLine, log.Severity, log.Target, log.TimeStamp, string.Empty, "---- BEGIN LOG", string.Empty, log.Message, string.Empty, log.FormattedMessage }));
			return fileInfo.Name;
		}

		private string Line(LogEntry log, string text = null, params object[] args)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = string.Concat(" ", string.Format(text, args));
			}
			return this.Line(log.TimeStamp, string.Format("[{0}]{1}", log, text), new object[0]);
		}

		private string Line(DateTimeOffset timestamp, string text, params object[] args)
		{
			return string.Concat(Environment.NewLine, string.Format("[{0:HH:mm:ss}] {1}", timestamp.LocalDateTime, string.Format(text, args)));
		}

		protected override void Update(TaskLog log)
		{
			FileInfo fileInfo = this.EnsureFilePath(log);
			StringBuilder stringBuilder = new StringBuilder();
			if (log.ErrorLog != null)
			{
				stringBuilder.Append(this.ErrorLine(log.ErrorLog, log.Name));
			}
			stringBuilder.Append(this.EndLine(log));
			File.AppendAllText(fileInfo.FullName, stringBuilder.ToString());
		}

		protected override void Update(StepLog log)
		{
			FileInfo fileInfo = this.EnsureFilePath(log.TaskLog);
			StringBuilder stringBuilder = new StringBuilder();
			if (log.ErrorLog != null)
			{
				stringBuilder.Append(this.ErrorLine(log.ErrorLog, log.Name));
			}
			stringBuilder.Append(this.EndLine(log));
			File.AppendAllText(fileInfo.FullName, stringBuilder.ToString());
		}
	}
}