using System;

namespace Vertica.Integration.Model.Hosting
{
	public interface IHost
	{
		string Description
		{
			get;
		}

		bool CanHandle(HostArguments args);

		void Handle(HostArguments args);
	}
}