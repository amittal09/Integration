using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MessageContainsText : Specification<MonitorEntry>
	{
		private readonly string[] _texts;

		public MessageContainsText(params string[] texts)
		{
			this._texts = texts ?? new string[0];
		}

		public override bool IsSatisfiedBy(MonitorEntry context)
		{
			return this._texts.Any<string>((string text) => {
				if (context.Message == null)
				{
					return false;
				}
				return context.Message.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
			});
		}
	}
}