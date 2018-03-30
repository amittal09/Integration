using System;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class ArchiveFoldersStep : Step<MaintenanceWorkItem>
	{
		private readonly IArchiveService _archiver;

		public override string Description
		{
			get
			{
				return "Archives files/folders based on configuration (MaintenanceConfiguration)";
			}
		}

		public ArchiveFoldersStep(IArchiveService archiver)
		{
			this._archiver = archiver;
		}

		public override Execution ContinueWith(MaintenanceWorkItem workItem)
		{
			if (workItem.Configuration.ArchiveFolders.GetEnabledFolders().Length == 0)
			{
				return Execution.StepOver;
			}
			return Execution.Execute;
		}

		public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
		{
			int j;
			MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] enabledFolders = workItem.Configuration.ArchiveFolders.GetEnabledFolders();
			for (int num = 0; num < (int)enabledFolders.Length; num++)
			{
				MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder folder = enabledFolders[num];
				context.Log.Message("Folder: {0}", new object[] { folder });
				FileInfo[] files = folder.GetFiles() ?? new FileInfo[0];
				DirectoryInfo[] folders = folder.GetFolders() ?? new DirectoryInfo[0];
				if (files.Length != 0 || folders.Length != 0)
				{
					MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder folder1 = folder;
					ArchiveCreated archiveCreated = this._archiver.Archive(folder.ArchiveOptions.Name, (BeginArchive a) => {
						int i;
						if (folder1.ArchiveOptions.Expires.HasValue)
						{
							a.Options.ExpiresOn(folder1.ArchiveOptions.Expires.Value);
						}
						a.Options.GroupedBy(folder1.ArchiveOptions.GroupName);
						FileInfo[] fileInfoArray = files;
						for (i = 0; i < (int)fileInfoArray.Length; i++)
						{
							a.IncludeFile(fileInfoArray[i]);
						}
						DirectoryInfo[] directoryInfoArray = folders;
						for (i = 0; i < (int)directoryInfoArray.Length; i++)
						{
							a.IncludeFolder(directoryInfoArray[i]);
						}
					});
					context.Log.Message("{0} file(s) and {1} folder(s) have been archived and will now be physically deleted. {2}.", new object[] { (int)files.Length, (int)folders.Length, archiveCreated });
					FileInfo[] fileInfoArray1 = files;
					for (j = 0; j < (int)fileInfoArray1.Length; j++)
					{
						FileInfo fileInfo = fileInfoArray1[j];
						try
						{
							fileInfo.Delete();
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							throw new Exception(string.Format("Unable to delete file '{0}'.", fileInfo.FullName), exception);
						}
					}
					DirectoryInfo[] directoryInfoArray1 = folders;
					for (j = 0; j < (int)directoryInfoArray1.Length; j++)
					{
						DirectoryInfo directoryInfo = directoryInfoArray1[j];
						try
						{
							directoryInfo.Delete(true);
						}
						catch (Exception exception3)
						{
							Exception exception2 = exception3;
							throw new Exception(string.Format("Unable to delete folder '{0}'.", directoryInfo.FullName), exception2);
						}
					}
				}
			}
		}
	}
}