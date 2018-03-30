using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model
{
	public abstract class Task<TWorkItem> : ITask<TWorkItem>, ITask
	{
		private readonly IEnumerable<IStep<TWorkItem>> _steps;

		[JsonProperty(Order=2)]
		public abstract string Description
		{
			get;
		}

		[JsonProperty(Order=1)]
		public string Name
		{
			get
			{
				return this.Name();
			}
		}

		[JsonProperty(Order=3)]
		public IEnumerable<IStep<TWorkItem>> Steps
		{
			get
			{
				return this._steps;
			}
		}

		IEnumerable<IStep> Vertica.Integration.Model.ITask.Steps
		{
			get
			{
				return this.Steps;
			}
		}

		protected Task(IEnumerable<IStep<TWorkItem>> steps)
		{
			this._steps = steps ?? Enumerable.Empty<IStep<TWorkItem>>();
		}

		public virtual void End(TWorkItem workItem, ITaskExecutionContext context)
		{
		}

		public abstract TWorkItem Start(ITaskExecutionContext context);
	}
}