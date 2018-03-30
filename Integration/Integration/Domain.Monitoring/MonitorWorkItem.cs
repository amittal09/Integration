using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorWorkItem : ContextWorkItem
	{
		private readonly static CultureInfo English;

		private readonly Range<DateTimeOffset> _checkRange;

		private readonly List<Tuple<Target, MonitorEntry>> _entries;

		private readonly List<ISpecification<MonitorEntry>> _ignore;

		private readonly ChainOfResponsibilityLink<MonitorEntry, Target[]> _redirects;

		private readonly List<Regex> _messageGrouping;

		public Range<DateTimeOffset> CheckRange
		{
			get
			{
				return this._checkRange;
			}
		}

		public MonitorConfiguration Configuration
		{
			get;
		}

		static MonitorWorkItem()
		{
			MonitorWorkItem.English = CultureInfo.GetCultureInfo("en-US");
		}

		public MonitorWorkItem(MonitorConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			DateTimeOffset utcNow = Time.UtcNow;
			if (configuration.LastRun > utcNow)
			{
				utcNow = configuration.LastRun;
			}
			this._checkRange = new Range<DateTimeOffset>(configuration.LastRun, utcNow);
			this._entries = new List<Tuple<Target, MonitorEntry>>();
			this._ignore = new List<ISpecification<MonitorEntry>>();
			this._redirects = ChainOfResponsibility.Empty<MonitorEntry, Target[]>();
			this._messageGrouping = new List<Regex>();
			this.Configuration = configuration;
		}

		public MonitorWorkItem Add(MonitorEntry entry, params Target[] targets)
		{
			Target[] targetArray;
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (!this._ignore.Any<ISpecification<MonitorEntry>>((ISpecification<MonitorEntry> x) => x.IsSatisfiedBy(entry)))
			{
				targets = targets ?? new Target[0];
				if (this._redirects.TryHandle(entry, out targetArray))
				{
					Target[] targetArray1 = targets;
					Target[] targetArray2 = targetArray ?? new Target[0];
					targets = ((IEnumerable<Target>)targetArray1).Concat<Target>((IEnumerable<Target>)targetArray2).ToArray<Target>();
				}
				if (targets.Length == 0)
				{
					targets = new Target[] { Target.Service };
				}
				foreach (Target target in targets.Distinct<Target>())
				{
					this._entries.Add(Tuple.Create<Target, MonitorEntry>(target ?? Target.Service, entry));
				}
			}
			return this;
		}

		public void Add(DateTimeOffset dateTime, string source, string message, params Target[] targets)
		{
			this.Add(new MonitorEntry(dateTime, source, message), targets);
		}

		public MonitorWorkItem AddIgnoreFilter(ISpecification<MonitorEntry> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
			this._ignore.Add(filter);
			return this;
		}

		public MonitorWorkItem AddMessageGroupingPatterns(params string[] regexPatterns)
		{
			string[] strArrays = regexPatterns ?? new string[0];
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				this._messageGrouping.Add(new Regex(str, RegexOptions.IgnoreCase));
			}
			return this;
		}

		public MonitorWorkItem AddTargetRedirect(IChainOfResponsibilityLink<MonitorEntry, Target[]> redirect)
		{
			if (redirect == null)
			{
				throw new ArgumentNullException("redirect");
			}
			this._redirects.Chain(redirect);
			return this;
		}

		public MonitorEntry[] GetEntries(Target target)
		{
			return (
				from x in this._entries
				where ((IEnumerable<Target>)(new Target[] { target, Target.All })).Any<Target>((Target t) => x.Item1.Equals(t))
				select x.Item2 into x
				orderby x.DateTime descending
				select x).GroupBy<MonitorEntry, MonitorEntry>((MonitorEntry x) => x, new MonitorWorkItem.MonitorEntryGrouping(this._messageGrouping)).Select<IGrouping<MonitorEntry, MonitorEntry>, MonitorEntry>((IGrouping<MonitorEntry, MonitorEntry> x) => {
				if (x.Count<MonitorEntry>() == 1)
				{
					return x.Key;
				}
				return this.Group(x.ToArray<MonitorEntry>());
			}).ToArray<MonitorEntry>();
		}

		private MonitorEntry Group(MonitorEntry[] entries)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(entries[0].Message);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Format("--- Additional similar entries (Total: {0}) ---", (int)entries.Length));
			stringBuilder.AppendLine();
			foreach (MonitorEntry monitorEntry in entries.Skip<MonitorEntry>(1))
			{
				foreach (Regex regex in this._messageGrouping)
				{
					Match match = regex.Match(monitorEntry.Message);
					if (!match.Success)
					{
						continue;
					}
					stringBuilder.AppendFormat("{0} ", match.Value);
				}
				DateTimeOffset dateTime = monitorEntry.DateTime;
				stringBuilder.AppendLine(string.Format("({0})", dateTime.ToString(MonitorWorkItem.English)));
			}
			return new MonitorEntry(entries[0].DateTime, entries[0].Source, stringBuilder.ToString().Trim());
		}

		public bool HasEntriesForUnconfiguredTargets(out Target[] targets)
		{
			Target[] array = (
				from x in this._entries
				select x.Item1).Distinct<Target>().ToArray<Target>();
			Target[] targetArray = ((IEnumerable)(this.Configuration.Targets ?? new MonitorTarget[0])).Cast<Target>().Distinct<Target>().ToArray<Target>();
			targets = array.Except<Target>(targetArray.Concat<Target>((IEnumerable<Target>)(new Target[] { Target.All }))).ToArray<Target>();
			return targets.Length != 0;
		}

		private class MonitorEntryGrouping : IEqualityComparer<MonitorEntry>
		{
			private readonly List<Regex> _groupings;

			public MonitorEntryGrouping(List<Regex> groupings)
			{
				this._groupings = groupings;
			}

			public bool Equals(MonitorEntry x, MonitorEntry y)
			{
				if (!string.Equals(x.Source, y.Source))
				{
					return false;
				}
				string message = x.Message;
				string str = y.Message;
				foreach (Regex _grouping in this._groupings)
				{
					message = _grouping.Replace(message, string.Empty);
					str = _grouping.Replace(str, string.Empty);
				}
				return string.Equals(message, str);
			}

			public int GetHashCode(MonitorEntry obj)
			{
				return 0;
			}
		}
	}
}