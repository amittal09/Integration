using System;
using System.Runtime.Serialization;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Exceptions
{
	[Serializable]
	public class NoHostFoundException : Exception
	{
		public NoHostFoundException()
		{
		}

		internal NoHostFoundException(HostArguments args) : base(string.Format("None of the configured {0} are able to handle arguments: \r\n{1}\r\n\r\nIf you are trying to execute a Task, make sure that you are spelling the name of the Task correctly.\r\n\r\nConsider running the \"WriteDocumentationTask\" to get a text-output of available tasks and hosts.\r\n\r\nTo configure Hosts in general, use the .Hosts(hosts => hosts.Host<YourHost>) method part of the initial configuration.", typeof(IHost).FullName, args))
		{
		}

		protected NoHostFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}