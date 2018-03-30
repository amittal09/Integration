using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Vertica.Integration.Infrastructure.IO
{
	public class FilesChangedHandler : IProcessExitHandler
	{
		private readonly ManualResetEvent _waitHandle;

		public FilesChangedHandler(FileInfo fileToWatch) : this((fileToWatch != null ? fileToWatch.FullName : null))
		{
		}

		public FilesChangedHandler(DirectoryInfo directoryToWatch) : this((directoryToWatch != null ? directoryToWatch.FullName : null))
		{
		}

		private FilesChangedHandler(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("Value cannot be null or empty", "path");
			}
			this._waitHandle = new ManualResetEvent(false);
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(path);
			fileSystemWatcher.Created += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => this._waitHandle.Set());
			fileSystemWatcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => this._waitHandle.Set());
			fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
			fileSystemWatcher.IncludeSubdirectories = false;
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		public void Wait()
		{
			this._waitHandle.WaitOne();
		}
	}
}