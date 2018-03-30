using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public interface IArchiveService
	{
		BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null);

		int Delete(DateTimeOffset olderThan);

		int DeleteExpired();

		byte[] Get(string id);

		Archive[] GetAll();
	}
}