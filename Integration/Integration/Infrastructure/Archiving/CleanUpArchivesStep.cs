using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class CleanUpArchivesStep : Step<MaintenanceWorkItem>
	{
		private readonly IArchiveService _archive;

		public override string Description
		{
			get
			{
				return "Deletes expired archives.";
			}
		}

		public CleanUpArchivesStep(IArchiveService archive)
		{
			this._archive = archive;
		}

		public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
		{
			int num = this._archive.DeleteExpired();
			if (num > 0)
			{
				context.Log.Message("Deleted {0} expired archive(s).", new object[] { num });
			}
		}
	}
}