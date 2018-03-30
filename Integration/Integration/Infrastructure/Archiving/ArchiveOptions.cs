using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class ArchiveOptions
	{
		[JsonProperty]
		public DateTimeOffset? Expires
		{
			get;
			private set;
		}

		[JsonProperty]
		public string GroupName
		{
			get;
			private set;
		}

		[JsonProperty]
		public string Name
		{
			get;
			private set;
		}

		public ArchiveOptions(string name)
		{
			this.Named(name);
		}

		public ArchiveOptions ExpiresAfter(TimeSpan timeSpan)
		{
			return this.ExpiresOn(Time.UtcNow.Add(timeSpan));
		}

		public ArchiveOptions ExpiresAfterDays(uint days)
		{
			return this.ExpiresAfter(TimeSpan.FromDays((double)((float)days)));
		}

		public ArchiveOptions ExpiresAfterMonths(uint months)
		{
			return this.ExpiresOn(Time.UtcNow.AddMonths((int)months));
		}

		public ArchiveOptions ExpiresOn(DateTimeOffset dateTime)
		{
			this.Expires = new DateTimeOffset?(dateTime);
			return this;
		}

		public ArchiveOptions GroupedBy(string name)
		{
			this.GroupName = name;
			return this;
		}

		public ArchiveOptions Named(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this.Name = name;
			return this;
		}
	}
}