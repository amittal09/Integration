using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Domain.Core
{
	public class WriteDocumentationTask : Task
	{
		private readonly ITaskFactory _taskFactory;

		private readonly IHostFactory _hostFactory;

		public override string Description
		{
			get
			{
				return "Outputs all integration tasks and related steps. Use argument \"ToFile\" to generate a text-file with this documentation.";
			}
		}

		public WriteDocumentationTask(ITaskFactory taskFactory, IHostFactory hostFactory)
		{
			this._taskFactory = taskFactory;
			this._hostFactory = hostFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			string str;
			int i;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine();
			Func<string, int, string> func = (string msg, int count) => string.Concat(new string(' ', count), msg);
			IHost[] all = this._hostFactory.GetAll();
			for (i = 0; i < (int)all.Length; i++)
			{
				IHost host = all[i];
				stringBuilder.AppendLine(host.Name());
				stringBuilder.AppendLine(func(host.Description, 3));
				stringBuilder.AppendLine();
			}
			ITask[] taskArray = this._taskFactory.GetAll();
			for (i = 0; i < (int)taskArray.Length; i++)
			{
				ITask task = taskArray[i];
				stringBuilder.AppendLine(task.Name());
				if (!string.IsNullOrWhiteSpace(task.Description))
				{
					stringBuilder.AppendLine(func(task.Description, 3));
				}
				stringBuilder.AppendLine();
				foreach (IStep step in task.Steps)
				{
					stringBuilder.AppendLine(func(step.Name(), 3));
					stringBuilder.AppendLine(func(step.Description, 6));
					stringBuilder.AppendLine();
				}
			}
			string str1 = stringBuilder.ToString();
			if (!context.Arguments.TryGetValue("ToFile", out str))
			{
				context.Log.Message(str1, new object[0]);
				return;
			}
			FileInfo fileInfo = new FileInfo(str ?? "Documentation.txt");
			File.WriteAllText(fileInfo.Name, str1);
			context.Log.Message("File generated. Location: {0}", new object[] { fileInfo.FullName });
		}
	}
}