using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vertica.Integration.Model;

namespace Vertica.Integration.Model.Hosting
{
	public class HostArguments
	{
		public Arguments Args
		{
			get;
		}

		public string Command
		{
			get;
		}

		public Arguments CommandArgs
		{
			get;
		}

		public HostArguments(string command, KeyValuePair<string, string>[] commandArgs, KeyValuePair<string, string>[] args)
		{
			this.Command = command;
			this.CommandArgs = new Arguments("-", commandArgs);
			this.Args = new Arguments(string.Empty, args);
		}

		public override string ToString()
		{
			return string.Format("{0} [CommandArgs = {1}] [Args = {2}]", this.Command, this.CommandArgs, this.Args);
		}
	}
}