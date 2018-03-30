using Castle.Windsor;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	public class TextFileLoggerConfiguration : IInitializable<IWindsorContainer>
	{
		private TextFileLoggerConfiguration.Organizer _organizer;

		internal TextFileLoggerConfiguration()
		{
		}

		private FileInfo Combine(string baseDirectory, DateTimeOffset timestamp, string postFixFormat, params object[] args)
		{
			string str;
			Thread.Sleep(1);
			string str1 = string.Format("{0:yyyyMMddHHmmss-fff}-{1}.txt", timestamp.LocalDateTime, string.Format(postFixFormat, args));
			if (this._organizer != null)
			{
				str = this._organizer.SubdirectoryName(timestamp);
			}
			else
			{
				str = null;
			}
			string str2 = str;
			return new FileInfo(Path.Combine(baseDirectory, str2 ?? string.Empty, str1));
		}

		internal FileInfo GetFilePath(TaskLog log, string baseDirectory)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			return this.Combine(baseDirectory, log.TimeStamp, "{0}", new object[] { log.Name });
		}

		internal FileInfo GetFilePath(ErrorLog log, string baseDirectory)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			return this.Combine(baseDirectory, log.TimeStamp, "{0}-{1}", new object[] { log.Severity, log.Target });
		}

		public TextFileLoggerConfiguration OrganizeSubFoldersBy(Func<TextFileLoggerConfiguration.BasedOn, TextFileLoggerConfiguration.Organizer> basedOn)
		{
			this._organizer = basedOn(new TextFileLoggerConfiguration.BasedOn());
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance<TextFileLoggerConfiguration>(this);
		}

		public class BasedOn
		{
			public TextFileLoggerConfiguration.Organizer Daily
			{
				get
				{
					return new TextFileLoggerConfiguration.DailyOrganizer();
				}
			}

			public TextFileLoggerConfiguration.Organizer Monthly
			{
				get
				{
					return new TextFileLoggerConfiguration.MonthlyOrganizer();
				}
			}

			public TextFileLoggerConfiguration.Organizer Weekly
			{
				get
				{
					return new TextFileLoggerConfiguration.WeeklyOrganizer();
				}
			}

			public BasedOn()
			{
			}

			public TextFileLoggerConfiguration.Organizer Custom(TextFileLoggerConfiguration.Organizer organizer)
			{
				if (organizer == null)
				{
					throw new ArgumentNullException("organizer");
				}
				return organizer;
			}
		}

		public class DailyOrganizer : TextFileLoggerConfiguration.Organizer
		{
			public DailyOrganizer()
			{
			}

			public override string SubdirectoryName(DateTimeOffset date)
			{
				return date.LocalDateTime.ToString("yyyyMMdd");
			}
		}

		public class MonthlyOrganizer : TextFileLoggerConfiguration.Organizer
		{
			public MonthlyOrganizer()
			{
			}

			public override string SubdirectoryName(DateTimeOffset date)
			{
				return date.LocalDateTime.ToString("yyyyMM");
			}
		}

		public abstract class Organizer
		{
			protected Organizer()
			{
			}

			public abstract string SubdirectoryName(DateTimeOffset date);
		}

		public class WeeklyOrganizer : TextFileLoggerConfiguration.Organizer
		{
			public WeeklyOrganizer()
			{
			}

			public override string SubdirectoryName(DateTimeOffset date)
			{
				int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.LocalDateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
				return string.Format("{0:yyyy}-{1:00}", date.LocalDateTime, weekOfYear);
			}
		}
	}
}