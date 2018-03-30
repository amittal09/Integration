using System;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class HandleAsWindowsService
	{
		public string Description
		{
			get;
			private set;
		}

		public string DisplayName
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		internal Func<IDisposable> OnStartFactory
		{
			get;
			private set;
		}

		public HandleAsWindowsService(string name, string displayName, string description, Func<IDisposable> onStartFactory = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			if (string.IsNullOrWhiteSpace(displayName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "displayName");
			}
			this.Name = name;
			this.DisplayName = displayName;
			this.Description = description.NullIfEmpty();
			Func<IDisposable> func = onStartFactory;
			if (func == null)
			{
			}
			this.OnStartFactory = () => new DisposableAction(() => {
			});
		}
	}
}