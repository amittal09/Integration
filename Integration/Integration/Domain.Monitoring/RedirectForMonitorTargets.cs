using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
	public class RedirectForMonitorTargets : IChainOfResponsibilityLink<MonitorEntry, Target[]>
	{
		private readonly Tuple<Target, MessageContainsText>[] _filters;

		public RedirectForMonitorTargets(params MonitorTarget[] targets)
		{
			this._filters = (
				from target in (IEnumerable<MonitorTarget>)(targets ?? new MonitorTarget[0])
				select Tuple.Create<Target, MessageContainsText>(target, new MessageContainsText(target.ReceiveErrorsWithMessagesContaining))).ToArray<Tuple<Target, MessageContainsText>>();
		}

		public bool CanHandle(MonitorEntry context)
		{
			return this._filters.Any<Tuple<Target, MessageContainsText>>((Tuple<Target, MessageContainsText> x) => x.Item2.IsSatisfiedBy(context));
		}

		public Target[] DoHandle(MonitorEntry context)
		{
			return (
				from x in this._filters
				where x.Item2.IsSatisfiedBy(context)
				select x.Item1).ToArray<Target>();
		}
	}
}