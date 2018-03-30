using System;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public interface IScheduledTaskHandler
	{
		bool Handle(HostArguments args, ITask task);
	}
}