using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public static class ArchiveExtensions
	{
		public static ArchiveCreated Archive(this IArchiveService service, string name, Action<BeginArchive> archive)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("service");
			}
			ArchiveCreated archiveCreated = null;
			using (BeginArchive beginArchive = service.Create(name, (ArchiveCreated x) => archiveCreated = x))
			{
				archive(beginArchive);
			}
			return archiveCreated;
		}

		public static ArchiveCreated ArchiveFile(this IArchiveService service, FileInfo file, Action<ArchiveOptions> options = null)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			ArchiveCreated archiveCreated = null;
			using (BeginArchive beginArchive = service.Create(file.Name, (ArchiveCreated x) => archiveCreated = x))
			{
				if (options != null)
				{
					options(beginArchive.Options);
				}
				beginArchive.IncludeFile(file);
			}
			return archiveCreated;
		}

		public static ArchiveCreated ArchiveFolder(this IArchiveService service, DirectoryInfo folder, Action<ArchiveOptions> options = null)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			ArchiveCreated archiveCreated = null;
			using (BeginArchive beginArchive = service.Create(folder.Name, (ArchiveCreated x) => archiveCreated = x))
			{
				if (options != null)
				{
					options(beginArchive.Options);
				}
				beginArchive.IncludeFolder(folder);
			}
			return archiveCreated;
		}

		public static ArchiveCreated ArchiveObjectAsJson(this IArchiveService service, object obj, string name, Action<ArchiveOptions> options = null)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			ArchiveCreated archiveCreated = null;
			using (BeginArchive beginArchive = service.Create(name, (ArchiveCreated x) => archiveCreated = x))
			{
				if (options != null)
				{
					options(beginArchive.Options);
				}
				beginArchive.IncludeObjectAsJson(obj, name);
			}
			return archiveCreated;
		}

		public static ArchiveCreated ArchiveText(this IArchiveService service, string name, string content, Action<ArchiveOptions> options = null)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			ArchiveCreated archiveCreated = null;
			using (BeginArchive beginArchive = service.Create(name, (ArchiveCreated x) => archiveCreated = x))
			{
				if (options != null)
				{
					options(beginArchive.Options);
				}
				beginArchive.IncludeContent(name, content, null);
			}
			return archiveCreated;
		}
	}
}