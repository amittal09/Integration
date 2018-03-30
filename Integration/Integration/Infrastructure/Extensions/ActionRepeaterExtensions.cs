using System;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class ActionRepeaterExtensions
	{
		public static ActionRepeater Repeat(this TimeSpan delay, Action action, TextWriter outputter = null)
		{
			return action.Repeat(delay, outputter);
		}

		public static ActionRepeater Repeat(this Action action, TimeSpan delay, TextWriter outputter = null)
		{
			return ActionRepeater.Start(action, delay, outputter);
		}
	}
}