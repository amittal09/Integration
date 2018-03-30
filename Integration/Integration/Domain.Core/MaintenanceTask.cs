using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class MaintenanceTask : Task<MaintenanceWorkItem>
	{
		private readonly IConfigurationService _service;

		public override string Description
		{
			get
			{
				return "Performs a number of steps to clean up the solution.";
			}
		}

		public MaintenanceTask(IEnumerable<IStep<MaintenanceWorkItem>> steps, IConfigurationService service) : base(steps)
		{
			this._service = service;
		}

		public override MaintenanceWorkItem Start(ITaskExecutionContext context)
		{
			return new MaintenanceWorkItem(this._service.Get<MaintenanceConfiguration>());
		}
	}
}