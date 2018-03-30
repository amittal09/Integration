using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class DumpArchiveTask : Task
	{
		private readonly IArchiveService _archive;

		public override string Description
		{
			get
			{
				return "Dumps a specified archive to the file system.";
			}
		}

		public DumpArchiveTask(IArchiveService archive)
		{
			this._archive = archive;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			//foreach (string str in 
			//	from x in context.Arguments
			//	select x.Key)
			//{
			//	byte[] numArray = this._archive.Get(str);
			//	if (numArray == null)
			//	{
			//		context.Log.Warning(Target.Service, "Archive '{0}' not found.", new object[] { str });
			//	}
			//	else
			//	{
			//		string str1 = Path.Combine(Directory.CreateDirectory("Archive-Dumps").FullName, string.Format("{0}.zip", str));
			//		File.WriteAllBytes(str1, numArray);
			//		context.Log.Message("Archive dumped to {0}.", new object[] { str1 });
			//	}
			//}
		}
	}
}