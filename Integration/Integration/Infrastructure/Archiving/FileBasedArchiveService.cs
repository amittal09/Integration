using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
	internal class FileBasedArchiveService : IArchiveService
	{
		private readonly string _baseDirectory;

		private readonly ILogger _logger;

		public const string BaseDirectoryKey = "FileBasedArchiveService.BaseDirectory";

		public FileBasedArchiveService(IRuntimeSettings settings, ILogger logger)
		{
			this._baseDirectory = settings["FileBasedArchiveService.BaseDirectory"].NullIfEmpty() ?? "Data\\Archives";
			if (!Directory.Exists(this._baseDirectory))
			{
				Directory.CreateDirectory(this._baseDirectory);
			}
			this._logger = logger;
		}

		private IEnumerable<FileInfo> Archives()
		{
			return (new DirectoryInfo(this._baseDirectory)).EnumerateFiles("*.zip", SearchOption.AllDirectories);
		}

		public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
		{
			return new BeginArchive(name, (MemoryStream stream, ArchiveOptions options) => {
				string str = Guid.NewGuid().ToString("N");
				Directory.CreateDirectory(this._baseDirectory);
				FileInfo fileInfo = new FileInfo(Path.Combine(this._baseDirectory, string.Format("{0}.zip", str)));
				File.WriteAllBytes(fileInfo.FullName, stream.ToArray());
				File.WriteAllText(FileBasedArchiveService.MetaFilePath(fileInfo).FullName, (new FileBasedArchiveService.MetaFile(options)).ToString());
				if (onCreated != null)
				{
					onCreated(new ArchiveCreated(str, new string[0]));
				}
			});
		}

		public int Delete(DateTimeOffset olderThan)
		{
			int num = 0;
			foreach (FileInfo fileInfo in this.Archives())
			{
				if (fileInfo.CreationTimeUtc > olderThan)
				{
					continue;
				}
				FileBasedArchiveService.DeleteArchive(fileInfo);
				num++;
			}
			return num;
		}

		private static void DeleteArchive(FileInfo archiveFile)
		{
			FileInfo fileInfo = FileBasedArchiveService.MetaFilePath(archiveFile);
			archiveFile.Delete();
			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}

		public int DeleteExpired()
		{
			int num = 0;
			foreach (FileInfo fileInfo in this.Archives())
			{
				if (this.Map(fileInfo).Expires.GetValueOrDefault(DateTimeOffset.MaxValue) > Time.UtcNow)
				{
					continue;
				}
				FileBasedArchiveService.DeleteArchive(fileInfo);
				num++;
			}
			return num;
		}

		public byte[] Get(string id)
		{
			string str = Path.Combine(this._baseDirectory, string.Format("{0}.zip", id));
			if (!File.Exists(str))
			{
				return null;
			}
			return File.ReadAllBytes(str);
		}

		public Archive[] GetAll()
		{
			return this.Archives().Select<FileInfo, Archive>(new Func<FileInfo, Archive>(this.Map)).ToArray<Archive>();
		}

		private Archive Map(FileInfo archiveFile)
		{
			string groupName;
			DateTimeOffset? expires;
			FileBasedArchiveService.MetaFile metaFile = this.ReadMetaFile(archiveFile);
			Archive archive = new Archive()
			{
				Id = Path.GetFileNameWithoutExtension(archiveFile.Name),
				Created = archiveFile.CreationTimeUtc,
				ByteSize = archiveFile.Length,
				Name = (metaFile != null ? metaFile.Name : archiveFile.Name)
			};
			if (metaFile != null)
			{
				groupName = metaFile.GroupName;
			}
			else
			{
				groupName = null;
			}
			archive.GroupName = groupName;
			if (metaFile != null)
			{
				expires = metaFile.Expires;
			}
			else
			{
				expires = null;
			}
			archive.Expires = expires;
			return archive;
		}

		private static FileInfo MetaFilePath(FileInfo archiveFile)
		{
			return new FileInfo(Path.Combine(new string[] { string.Format("{0}.meta", archiveFile.FullName) }));
		}

		private FileBasedArchiveService.MetaFile ReadMetaFile(FileInfo archiveFile)
		{
			FileInfo fileInfo = FileBasedArchiveService.MetaFilePath(archiveFile);
			if (!fileInfo.Exists)
			{
				return null;
			}
			string str = File.ReadAllText(fileInfo.FullName);
			if (string.IsNullOrWhiteSpace(str))
			{
				return null;
			}
			return FileBasedArchiveService.MetaFile.FromJson(str, this._logger);
		}

		private class MetaFile
		{
			public DateTimeOffset? Expires
			{
				get;
				set;
			}

			public string GroupName
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public MetaFile(ArchiveOptions options = null)
			{
				if (options != null)
				{
					this.Name = options.Name;
					this.GroupName = options.GroupName;
					this.Expires = options.Expires;
				}
			}

			public static FileBasedArchiveService.MetaFile FromJson(string json, ILogger logger)
			{
				FileBasedArchiveService.MetaFile metaFile;
				if (string.IsNullOrWhiteSpace(json))
				{
					throw new ArgumentException("Value cannot be null or empty.", "json");
				}
				if (logger == null)
				{
					throw new ArgumentNullException("logger");
				}
				try
				{
					metaFile = JsonConvert.DeserializeObject<FileBasedArchiveService.MetaFile>(json);
				}
				catch (Exception exception)
				{
					logger.LogError(exception, null);
					metaFile = null;
				}
				return metaFile;
			}

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this, Formatting.Indented);
			}
		}
	}
}