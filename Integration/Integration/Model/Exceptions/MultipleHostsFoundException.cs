using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Exceptions
{
	[Serializable]
	public class MultipleHostsFoundException : Exception
	{
		public MultipleHostsFoundException()
		{
		}

		internal MultipleHostsFoundException(HostArguments args, IHost[] hosts) : 
            base(string.Format("Multiple hosts was found to handle arguments: \r\n{0}\r\n\r\nThe hosts are:\r\n\r\n{1}\r\n\r\nTo fix this problem you need to inspect the \"CanHandle\"-method of the hosts to find out why the criteria is met.", args, string.Join(", ", ((IEnumerable<IHost>)hosts).Select<IHost, string>((IHost x) => x.Name()))))
		{
		}

		protected MultipleHostsFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}