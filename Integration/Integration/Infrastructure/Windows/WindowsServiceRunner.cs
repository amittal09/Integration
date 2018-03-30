using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class WindowsServiceRunner : ServiceBase
	{
		private readonly string _name;

		private readonly Func<IDisposable> _onStartFactory;

		private IDisposable _current;

		public new string ServiceName
		{
			get
			{
				return this._name;
			}
		}

		public WindowsServiceRunner(string name, Func<IDisposable> onStartFactory)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this._name = name;
			this._onStartFactory = onStartFactory;
		}

		protected override void OnStart(string[] args)
		{
			if (this._current != null)
			{
				throw new InvalidOperationException("Cannot start when already running.");
			}
			this._current = this._onStartFactory();
		}

		protected override void OnStop()
		{
			if (this._current != null)
			{
				this._current.Dispose();
				this._current = null;
			}
		}
	}
}