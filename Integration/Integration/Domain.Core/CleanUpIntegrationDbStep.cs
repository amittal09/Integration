using System;
using System.Data;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
		private readonly IDbFactory _db;

		private readonly IConfigurationService _configuration;

		private readonly IArchiveService _archiver;

		public override string Description
		{
			get
			{
				MaintenanceConfiguration maintenanceConfiguration = this._configuration.Get<MaintenanceConfiguration>();
				object totalDays = maintenanceConfiguration.CleanUpTaskLogEntriesOlderThan.TotalDays;
				TimeSpan cleanUpErrorLogEntriesOlderThan = maintenanceConfiguration.CleanUpErrorLogEntriesOlderThan;
				return string.Format("Deletes TaskLog entries older than {0} days and ErrorLog entries older than {1} days from IntegrationDb.", totalDays, cleanUpErrorLogEntriesOlderThan.TotalDays);
			}
		}

		public CleanUpIntegrationDbStep(IDbFactory db, IConfigurationService configuration, IArchiveService archiver)
		{
			this._db = db;
			this._configuration = configuration;
			this._archiver = archiver;
		}

		private Tuple<int, string> DeleteEntries(IDbSession session, string tableName, DateTimeOffset lowerBound)
		{
			return session.Wrap<Tuple<int, string>>((IDbSession s) => {
				string str = string.Format(" FROM [{0}] WHERE [TimeStamp] <= @lowerbound", tableName);
				string csv = s.QueryToCsv(string.Concat("SELECT *", str), new { lowerBound = lowerBound }, null);
				return Tuple.Create<int, string>(s.Execute(string.Concat("DELETE", str), new { lowerBound = lowerBound }), csv);
			});
		}

		public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
		{
			DateTimeOffset utcNow = Time.UtcNow;
			DateTimeOffset dateTimeOffset = utcNow.Subtract(workItem.Configuration.CleanUpTaskLogEntriesOlderThan);
			utcNow = Time.UtcNow;
			DateTimeOffset dateTimeOffset1 = utcNow.Subtract(workItem.Configuration.CleanUpErrorLogEntriesOlderThan);
			using (IDbSession dbSession = this._db.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					Tuple<int, string> tuple = this.DeleteEntries(dbSession, "TaskLog", dateTimeOffset);
					Tuple<int, string> tuple1 = this.DeleteEntries(dbSession, "ErrorLog", dateTimeOffset1);
					dbTransaction.Commit();
					if (tuple.Item1 > 0 || tuple1.Item1 > 0)
					{
						ArchiveCreated archiveCreated = this._archiver.Archive("IntegrationDb-Maintenance", (BeginArchive a) => {
							a.Options.GroupedBy("Backup").ExpiresAfterMonths(12);
							a.IncludeContent(string.Format("TaskLog_{0:yyyyMMdd}.csv", dateTimeOffset), tuple.Item2, null);
							a.IncludeContent(string.Format("ErrorLog_{0:yyyyMMdd}.csv", dateTimeOffset1), tuple1.Item2, null);
						});
						context.Log.Message("Deleted {0} task entries older than '{1}'. \r\nDeleted {2} error entries older than '{3}'\r\nArchive: {4}", new object[] { tuple.Item1, dateTimeOffset, tuple1.Item1, dateTimeOffset1, archiveCreated });
					}
				}
			}
		}
	}
}