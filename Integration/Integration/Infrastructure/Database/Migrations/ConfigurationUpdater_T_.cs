using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class ConfigurationUpdater<T> : IDisposable
	where T : class, new()
	{
		private readonly Action<T> _save;

		public T Configuration
		{
			get;
		}

		internal ConfigurationUpdater(T configuration, Action<T> save)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			if (save == null)
			{
				throw new ArgumentNullException("save");
			}
			this.Configuration = configuration;
			this._save = save;
		}

		public void Dispose()
		{
			this._save(this.Configuration);
		}
	}
}