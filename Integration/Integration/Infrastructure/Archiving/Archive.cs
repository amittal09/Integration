using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class Archive
	{
		public long ByteSize
		{
			get;
			set;
		}

		public DateTimeOffset Created
		{
			get;
			set;
		}

		public DateTimeOffset? Expires
		{
			get;
			set;
		}

		public string GroupName
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public Archive()
		{
		}
	}
}