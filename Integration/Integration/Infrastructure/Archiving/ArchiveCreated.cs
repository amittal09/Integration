using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class ArchiveCreated
	{
		private readonly string[] _additionalDownloadOptions;

		public string Id
		{
			get;
		}

		public ArchiveCreated(string id, params string[] additionalDownloadOptions)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("Value cannot be null or empty.", "id");
			}
			this.Id = id;
			this._additionalDownloadOptions = additionalDownloadOptions ?? new string[0];
		}

		public static implicit operator String(ArchiveCreated archive)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("archive");
			}
			return archive.ToString();
		}

		public override string ToString()
		{
			return string.Format("ArchiveID: {0}\r\nOptions to download archive:\r\n{1}", this.Id, string.Join(Environment.NewLine, 
				from x in (new string[] { "From the web-based interface (Portal)", string.Format("Run the following command: {0} {1}", Task.NameOf<DumpArchiveTask>(), this.Id) }).Concat<string>(this._additionalDownloadOptions)
				select string.Format(" - {0}", x)));
		}
	}
}