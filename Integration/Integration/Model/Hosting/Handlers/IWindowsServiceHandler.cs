using System;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public interface IWindowsServiceHandler
	{
		bool Handle(HostArguments args, HandleAsWindowsService service);
	}
}