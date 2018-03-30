using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorFoldersStep : Step<MonitorWorkItem>
	{
		public override string Description
		{
			get
			{
				return "Monitors a set of configured folders (MonitorConfiguration).";
			}
		}

		public MonitorFoldersStep()
		{
		}

		public override Execution ContinueWith(MonitorWorkItem workItem)
		{
			if (workItem.Configuration.MonitorFolders.GetEnabledFolders().Length == 0)
			{
				return Execution.StepOver;
			}
			return Execution.Execute;
		}

		public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
		{
			MonitorConfiguration.MonitorFoldersConfiguration.Folder[] enabledFolders = workItem.Configuration.MonitorFolders.GetEnabledFolders();
			context.Log.Message("Folder(s) monitored:\r\n{0}", new object[] { string.Join(Environment.NewLine, 
				from x in (IEnumerable<MonitorConfiguration.MonitorFoldersConfiguration.Folder>)enabledFolders
				select string.Concat(" - ", x.ToString())) });
			MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folderArray = enabledFolders;
			for (int i = 0; i < (int)folderArray.Length; i++)
			{
				MonitorConfiguration.MonitorFoldersConfiguration.Folder folder = folderArray[i];
				IEnumerable<string> strs = Directory.EnumerateFiles(folder.Path, folder.SearchPattern ?? "*", (folder.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
				MonitorConfiguration.MonitorFoldersConfiguration.FileCriteria criteria = folder.Criteria;
				string[] array = strs.Where<string>(new Func<string, bool>(criteria.IsSatisfiedBy)).ToArray<string>();
				if (array.Length != 0)
				{
					context.Log.Message("{0} file(s) matched by '{1}'.", new object[] { (int)array.Length, folder });
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("{0} file(s) matching criteria: '{1}'.", (int)array.Length, folder.Criteria);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(string.Join(Environment.NewLine, 
						from x in array.Take<string>(10)
						select string.Format(" - {0}", x)));
					if ((int)array.Length > 10)
					{
						stringBuilder.AppendLine("...");
					}
					workItem.Add(Time.UtcNow, this.Name(), stringBuilder.ToString(), new Target[] { folder.Target });
				}
			}
		}
	}
}