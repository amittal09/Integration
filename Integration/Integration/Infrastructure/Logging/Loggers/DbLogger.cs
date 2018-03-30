using System;
using System.Data;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class DbLogger : Logger
	{
		private readonly IDbFactory _db;

		public DbLogger(IDbFactory db)
		{
			this._db = db;
		}

		protected override string Insert(TaskLog log)
		{
			int num = this.Persist((IDbSession session) => session.ExecuteScalar<int>("\r\nINSERT INTO TaskLog (Type, TaskName, TimeStamp, MachineName, IdentityName, CommandLine)\r\nVALUES ('T', @TaskName, @TimeStamp, @MachineName, @IdentityName, @CommandLine)\r\nSELECT CAST(SCOPE_IDENTITY() AS INT)", new { TaskName = log.Name, TimeStamp = log.TimeStamp, MachineName = log.MachineName, IdentityName = log.IdentityName, CommandLine = log.CommandLine }));
			return num.ToString();
		}

		protected override string Insert(MessageLog log)
		{
			int num = this.Persist((IDbSession session) => session.ExecuteScalar<int>("\r\nINSERT INTO TaskLog (Type, TaskName, TimeStamp, StepName, Message, TaskLog_Id, StepLog_Id)\r\nVALUES ('M', @TaskName, @TimeStamp, @StepName, @Message, @TaskLog_Id, @StepLog_Id)\r\nSELECT CAST(SCOPE_IDENTITY() AS INT)", new { TaskName = log.TaskLog.Name, TimeStamp = log.TimeStamp, StepName = (log.StepLog != null ? log.StepLog.Name : null), Message = log.Message, TaskLog_Id = log.TaskLog.Id, StepLog_Id = (log.StepLog != null ? log.StepLog.Id : null) }));
			return num.ToString();
		}

		protected override string Insert(StepLog log)
		{
			int num = this.Persist((IDbSession session) => session.ExecuteScalar<int>("\r\nINSERT INTO TaskLog (Type, TaskName, StepName, TimeStamp, TaskLog_Id)\r\nVALUES ('S', @TaskName, @StepName, @TimeStamp, @TaskLog_Id)\r\nSELECT CAST(SCOPE_IDENTITY() AS INT)", new { TaskName = log.TaskLog.Name, StepName = log.Name, TimeStamp = log.TimeStamp, TaskLog_Id = log.TaskLog.Id }));
			return num.ToString();
		}

		protected override string Insert(ErrorLog log)
		{
			int num = this.Persist((IDbSession session) => session.ExecuteScalar<int>("\r\nINSERT INTO [ErrorLog] (MachineName, IdentityName, CommandLine, Severity, Message, FormattedMessage, TimeStamp, Target)\r\nVALUES (@MachineName, @IdentityName, @CommandLine, @Severity, @Message, @FormattedMessage, @TimeStamp, @Target)\r\nSELECT CAST(SCOPE_IDENTITY() AS INT)", new { MachineName = log.MachineName, IdentityName = log.IdentityName, CommandLine = log.CommandLine, Severity = log.Severity.ToString(), Message = log.Message, FormattedMessage = log.FormattedMessage, TimeStamp = log.TimeStamp, Target = log.Target.ToString() }));
			return num.ToString();
		}

		private int Persist(Func<IDbSession, int> persist)
		{
			int num;
			using (IDbSession dbSession = this._db.OpenSession())
			{
				using (IDbTransaction dbTransaction = dbSession.BeginTransaction(null))
				{
					num = dbSession.Wrap<int>(persist);
					dbTransaction.Commit();
				}
			}
			return num;
		}

		protected override void Update(TaskLog log)
		{
			this.Persist((IDbSession session) => session.Execute("\r\nUPDATE TaskLog SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id", new { Id = log.Id, ExecutionTimeSeconds = log.ExecutionTimeSeconds.GetValueOrDefault(), ErrorLog_Id = (log.ErrorLog != null ? log.ErrorLog.Id : null) }));
		}

		protected override void Update(StepLog log)
		{
			this.Persist((IDbSession session) => session.Execute("\r\nUPDATE TaskLog SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id", new { Id = log.Id, ExecutionTimeSeconds = log.ExecutionTimeSeconds.GetValueOrDefault(), ErrorLog_Id = (log.ErrorLog != null ? log.ErrorLog.Id : null) }));
		}
	}
}