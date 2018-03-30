using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
	[Description("Used by the MonitorTask. Remember to set one or more recipients for each of the defined Targets.")]
	[Guid("9FF492BF-D4B5-4E67-AF72-C02EA8671051")]
	public class MonitorConfiguration
	{
		public string[] IgnoreErrorsWithMessagesContaining
		{
			get;
			set;
		}

		public DateTimeOffset LastRun
		{
			get;
			set;
		}

		public string[] MessageGroupingPatterns
		{
			get;
			set;
		}

		public MonitorConfiguration.MonitorFoldersConfiguration MonitorFolders
		{
			get;
			private set;
		}

		public MonitorConfiguration.PingUrlsConfiguration PingUrls
		{
			get;
			private set;
		}

		public string SubjectPrefix
		{
			get;
			set;
		}

		public MonitorTarget[] Targets
		{
			get;
			set;
		}

		public MonitorConfiguration()
		{
			this.Targets = new MonitorTarget[] { new MonitorTarget(Target.Service) };
			this.SubjectPrefix = "Integration Service";
			this.MonitorFolders = new MonitorConfiguration.MonitorFoldersConfiguration();
			this.PingUrls = new MonitorConfiguration.PingUrlsConfiguration();
		}

		internal void Assert()
		{
			if (this.Targets == null)
			{
				throw new InvalidOperationException("No targets defined for MonitorConfiguration.");
			}
			if (((IEnumerable<MonitorTarget>)this.Targets).SingleOrDefault<MonitorTarget>((MonitorTarget x) => x.Equals(Target.Service)) == null)
			{
				throw new InvalidOperationException(string.Format("Missing required target '{0}' for MonitorConfiguration.", Target.Service));
			}
		}

		public MonitorTarget EnsureMonitorTarget(ITarget target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			MonitorTarget[] targets = this.Targets ?? new MonitorTarget[0];
			MonitorTarget monitorTarget = targets.FirstOrDefault<MonitorTarget>((MonitorTarget x) => x.Equals(target));
			if (monitorTarget == null)
			{
				monitorTarget = new MonitorTarget(target.Name);
				this.Targets = targets.Concat<MonitorTarget>((IEnumerable<MonitorTarget>)(new MonitorTarget[] { monitorTarget })).ToArray<MonitorTarget>();
			}
			return monitorTarget;
		}

		public void RemoveTarget(ITarget target)
		{
			if (this.Targets == null)
			{
				return;
			}
			this.Targets = (
				from x in this.Targets
				where !x.Equals(target)
				select x).ToArray<MonitorTarget>();
		}

		public class MonitorFoldersConfiguration
		{
			public bool Enabled
			{
				get;
				set;
			}

			public MonitorConfiguration.MonitorFoldersConfiguration.Folder[] Folders
			{
				get;
				set;
			}

			public MonitorConfiguration.MonitorFoldersConfiguration.Folder this[int index]
			{
				get
				{
					this.EnsureFolders();
					return this.Folders[index];
				}
			}

			public MonitorFoldersConfiguration()
			{
				this.Enabled = true;
				this.EnsureFolders();
			}

			public void Add(Func<MonitorConfiguration.MonitorFoldersConfiguration.Folder, MonitorConfiguration.MonitorFoldersConfiguration.FileCriterias, MonitorConfiguration.MonitorFoldersConfiguration.FileCriteria> folder)
			{
				if (folder == null)
				{
					throw new ArgumentNullException("folder");
				}
				MonitorConfiguration.MonitorFoldersConfiguration.Folder folder1 = new MonitorConfiguration.MonitorFoldersConfiguration.Folder()
				{
					//Criteria = folder(folder1, new MonitorConfiguration.MonitorFoldersConfiguration.FileCriterias())
				};
				this.EnsureFolders();
				MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folders = this.Folders;
				MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folderArray = new MonitorConfiguration.MonitorFoldersConfiguration.Folder[] { folder1 };
				this.Folders = ((IEnumerable<MonitorConfiguration.MonitorFoldersConfiguration.Folder>)folders).Append<MonitorConfiguration.MonitorFoldersConfiguration.Folder>(folderArray).ToArray<MonitorConfiguration.MonitorFoldersConfiguration.Folder>();
			}

			public void Clear()
			{
				this.Folders = new MonitorConfiguration.MonitorFoldersConfiguration.Folder[0];
			}

			private void EnsureFolders()
			{
				if (this.Folders == null)
				{
					this.Folders = new MonitorConfiguration.MonitorFoldersConfiguration.Folder[0];
				}
			}

			public MonitorConfiguration.MonitorFoldersConfiguration.Folder[] GetEnabledFolders()
			{
				this.EnsureFolders();
				return this.Folders.Where<MonitorConfiguration.MonitorFoldersConfiguration.Folder>((MonitorConfiguration.MonitorFoldersConfiguration.Folder x) => {
					if (!this.Enabled || !x.Enabled || string.IsNullOrWhiteSpace(x.Path))
					{
						return false;
					}
					return x.Criteria != null;
				}).ToArray<MonitorConfiguration.MonitorFoldersConfiguration.Folder>();
			}

			public void Remove(MonitorConfiguration.MonitorFoldersConfiguration.Folder folder)
			{
				if (folder == null)
				{
					throw new ArgumentNullException("folder");
				}
				this.EnsureFolders();
				MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folders = this.Folders;
				MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folderArray = new MonitorConfiguration.MonitorFoldersConfiguration.Folder[] { folder };
				this.Folders = ((IEnumerable<MonitorConfiguration.MonitorFoldersConfiguration.Folder>)folders).Except<MonitorConfiguration.MonitorFoldersConfiguration.Folder>((IEnumerable<MonitorConfiguration.MonitorFoldersConfiguration.Folder>)folderArray).ToArray<MonitorConfiguration.MonitorFoldersConfiguration.Folder>();
			}

			public abstract class FileCriteria
			{
				protected FileCriteria()
				{
				}

				public abstract bool IsSatisfiedBy(string file);
			}

			public class FileCriterias
			{
				public FileCriterias()
				{
				}

				public MonitorConfiguration.MonitorFoldersConfiguration.FileCriteria FilesOlderThan(TimeSpan timeSpan)
				{
					return new MonitorConfiguration.MonitorFoldersConfiguration.FilesOlderThanCriteria((uint)timeSpan.TotalSeconds);
				}
			}

			public class FilesOlderThanCriteria : MonitorConfiguration.MonitorFoldersConfiguration.FileCriteria
			{
				public uint Seconds
				{
					get;
					set;
				}

				protected FilesOlderThanCriteria()
				{
				}

				internal FilesOlderThanCriteria(uint seconds)
				{
					this.Seconds = seconds;
				}

				public override bool IsSatisfiedBy(string file)
				{
					return (Time.UtcNow - File.GetLastWriteTimeUtc(file)) > TimeSpan.FromSeconds((double)((float)this.Seconds));
				}

				public override string ToString()
				{
					return string.Format("Files older than {0} second(s).", this.Seconds);
				}
			}

			public class Folder
			{
				public MonitorConfiguration.MonitorFoldersConfiguration.FileCriteria Criteria
				{
					get;
					set;
				}

				public bool Enabled
				{
					get;
					set;
				}

				public bool IncludeSubDirectories
				{
					get;
					set;
				}

				public string Path
				{
					get;
					set;
				}

				public string SearchPattern
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
				}

				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(this.Path);
					if (this.SearchPattern != null)
					{
						stringBuilder.AppendFormat(" (pattern = {0})", this.SearchPattern);
					}
					if (this.Criteria != null)
					{
						stringBuilder.AppendFormat(" (criteria = {0})", this.Criteria);
					}
					return stringBuilder.ToString();
				}
			}
		}

		public class PingUrlsConfiguration
		{
			public bool Enabled
			{
				get;
				set;
			}

			public uint MaximumWaitTimeSeconds
			{
				get;
				set;
			}

			internal bool ShouldExecute
			{
				get
				{
					if (!this.Enabled || this.Urls == null)
					{
						return false;
					}
					return this.Urls.Length != 0;
				}
			}

			public string[] Urls
			{
				get;
				set;
			}

			public PingUrlsConfiguration()
			{
				this.Enabled = true;
				this.MaximumWaitTimeSeconds = (uint)TimeSpan.FromMinutes(2).TotalSeconds;
			}
		}
	}
}