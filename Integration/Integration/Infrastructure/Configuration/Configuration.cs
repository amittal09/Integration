using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public class Configuration
	{
		public DateTimeOffset Created
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		public string JsonData
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public DateTimeOffset Updated
		{
			get;
			set;
		}

		public string UpdatedBy
		{
			get;
			set;
		}

		public Configuration()
		{
		}
	}
}