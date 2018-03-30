using System;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public interface IConfigurationService
	{
		ArchiveCreated Backup(string id);

		TConfiguration Get<TConfiguration>()
		where TConfiguration : class, new();

		TConfiguration Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false)
		where TConfiguration : class, new();
	}
}