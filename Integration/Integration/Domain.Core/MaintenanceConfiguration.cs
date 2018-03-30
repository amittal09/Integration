using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Core
{
	[Description("Used by the MaintenanceTask.")]
	[Guid("FBF783F5-0210-448D-BEB9-FD0E9AD6CABF")]
	public class MaintenanceConfiguration
	{
		public MaintenanceConfiguration.ArchiveFoldersConfiguration ArchiveFolders
		{
			get;
			private set;
		}

		[Obsolete("Each individual archive now controls their own expiration.")]
		public TimeSpan CleanUpArchivesOlderThan
		{
			get;
			set;
		}

		public TimeSpan CleanUpErrorLogEntriesOlderThan
		{
			get;
			set;
		}

		public TimeSpan CleanUpTaskLogEntriesOlderThan
		{
			get;
			set;
		}

		public MaintenanceConfiguration()
		{
			this.CleanUpTaskLogEntriesOlderThan = TimeSpan.FromDays(60);
			this.CleanUpErrorLogEntriesOlderThan = TimeSpan.FromDays(60);
			this.ArchiveFolders = new MaintenanceConfiguration.ArchiveFoldersConfiguration();
		}

		public class ArchiveFoldersConfiguration
		{
			public bool Enabled
			{
				get;
				set;
			}

			public MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] Folders
			{
				get;
				set;
			}

			public MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder this[int index]
			{
				get
				{
					this.EnsureFolders();
					return this.Folders[index];
				}
			}

			public ArchiveFoldersConfiguration()
			{
				this.Enabled = true;
				this.EnsureFolders();
			}

			public void Add(Func<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder, MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandlers, MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler> folder)
			{
				if (folder == null)
				{
					throw new ArgumentNullException("folder");
				}
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder folder1 = new MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder()
				{
					//Handler = folder(folder1, new MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandlers())
				};
				this.EnsureFolders();
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] folders = this.Folders;
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] folderArray = new MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] { folder1 };
				this.Folders = ((IEnumerable<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>)folders).Append<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>(folderArray).ToArray<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>();
			}

			public void Clear()
			{
				this.Folders = new MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[0];
			}

			private void EnsureFolders()
			{
				if (this.Folders == null)
				{
					this.Folders = new MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[0];
				}
			}

			public MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] GetEnabledFolders()
			{
				this.EnsureFolders();
				return this.Folders.Where<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>((MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder x) => {
					if (!this.Enabled || !x.Enabled || string.IsNullOrWhiteSpace(x.Path))
					{
						return false;
					}
					return x.Handler != null;
				}).ToArray<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>();
			}

			public void Remove(MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder folder)
			{
				if (folder == null)
				{
					throw new ArgumentNullException("folder");
				}
				this.EnsureFolders();
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] folders = this.Folders;
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] folderArray = new MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] { folder };
				this.Folders = ((IEnumerable<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>)folders).Except<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>((IEnumerable<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>)folderArray).ToArray<MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder>();
			}

			public class EverythingHandler : MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler
			{
				public EverythingHandler()
				{
				}

				public override IEnumerable<FileInfo> GetFiles(DirectoryInfo path)
				{
					return path.EnumerateFiles();
				}

				public override IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path)
				{
					return path.EnumerateDirectories();
				}

				public override string ToString()
				{
					return "All files and folders.";
				}
			}

			public class FilesOlderThanHandler : MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler
			{
				public bool IncludeSubDirectories
				{
					get;
					set;
				}

				public string SearchPattern
				{
					get;
					set;
				}

				public uint Seconds
				{
					get;
					set;
				}

				protected FilesOlderThanHandler()
				{
				}

				internal FilesOlderThanHandler(uint seconds, string searchPattern, bool includeSubDirectories)
				{
					this.Seconds = seconds;
					this.SearchPattern = searchPattern;
					this.IncludeSubDirectories = includeSubDirectories;
				}

				public override IEnumerable<FileInfo> GetFiles(DirectoryInfo path)
				{
					return 
						from x in path.EnumerateFiles(this.SearchPattern ?? "*", (this.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
						where (Time.UtcNow - x.LastWriteTimeUtc) > TimeSpan.FromSeconds((double)((float)this.Seconds))
						select x;
				}

				public override IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path)
				{
                    return null;
				}

				public override string ToString()
				{
					return string.Format("Files older than {0} second(s){1}{2}.", this.Seconds, (this.SearchPattern != null ? string.Format(" (Search Pattern = {0}", this.SearchPattern) : string.Empty), (this.IncludeSubDirectories ? " (All directories)" : string.Empty));
				}
			}

			public class Folder
			{
				public ArchiveOptions ArchiveOptions
				{
					get;
					private set;
				}

				public bool Enabled
				{
					get;
					set;
				}

				public MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler Handler
				{
					get;
					set;
				}

				public string Path
				{
					get;
					set;
				}

				public Target Target
				{
					get;
					set;
				}

				public Folder()
				{
					this.Enabled = true;
					this.Target = Target.Service;
					this.ArchiveOptions = (new ArchiveOptions(typeof(ArchiveFoldersStep).StepName())).GroupedBy("Backup");
				}

				public FileInfo[] GetFiles()
				{
					return (
						from x in this.Handler.GetFiles(new DirectoryInfo(this.Path))
						where x.Exists
						select x).ToArray<FileInfo>();
				}

				public DirectoryInfo[] GetFolders()
				{
					return (
						from x in this.Handler.GetFolders(new DirectoryInfo(this.Path))
						where x.Exists
						select x).ToArray<DirectoryInfo>();
				}

				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(this.Path);
					if (this.Handler != null)
					{
						stringBuilder.AppendFormat(" (handler = {0})", this.Handler);
					}
					return stringBuilder.ToString();
				}
			}

			public abstract class FolderHandler
			{
				protected FolderHandler()
				{
				}

				public abstract IEnumerable<FileInfo> GetFiles(DirectoryInfo path);

				public abstract IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path);
			}

			public class FolderHandlers
			{
				public FolderHandlers()
				{
				}

				public MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler Everything()
				{
					return new MaintenanceConfiguration.ArchiveFoldersConfiguration.EverythingHandler();
				}

				public MaintenanceConfiguration.ArchiveFoldersConfiguration.FolderHandler FilesOlderThan(TimeSpan timeSpan, string searchPattern = null, bool includeSubDirectories = false)
				{
					return new MaintenanceConfiguration.ArchiveFoldersConfiguration.FilesOlderThanHandler((uint)timeSpan.TotalSeconds, searchPattern, includeSubDirectories);
				}
			}
		}
	}
}