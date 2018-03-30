using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TaskScheduler;

namespace Vertica.Integration.Infrastructure.Windows
{
	public abstract class ScheduledTaskTrigger
	{
		private readonly DateTimeOffset _startDate;

		private readonly _TASK_TRIGGER_TYPE2 _type;

		private readonly TimeSpan? _interval;

		protected ScheduledTaskTrigger(DateTimeOffset startDate, _TASK_TRIGGER_TYPE2 type, TimeSpan? interval = null)
		{
			if (interval.HasValue && interval.Value.TotalDays > 31)
			{
				throw new ArgumentOutOfRangeException("interval", "Maximum interval is 31 days");
			}
			if (interval.HasValue && interval.Value.TotalSeconds < 60)
			{
				throw new ArgumentOutOfRangeException("interval", "Minimum interval is 1 minute");
			}
			this._startDate = startDate;
			this._type = type;
			this._interval = interval;
		}

		internal abstract void AddToTask(ITaskDefinition taskDefinition);

		protected T Create<T>(ITaskDefinition task)
		{
			ITrigger str = task.Triggers.Create(this._type);
			str.Enabled = true;
			str.StartBoundary = this._startDate.ToString("O");
			if (this._interval.HasValue)
			{
				IRepetitionPattern repetition = str.Repetition;
				object[] days = new object[4];
				TimeSpan value = this._interval.Value;
				days[0] = value.Days;
				value = this._interval.Value;
				days[1] = value.Hours;
				value = this._interval.Value;
				days[2] = value.Minutes;
				value = this._interval.Value;
				days[3] = value.Seconds;
				repetition.Interval = string.Format("P{0}DT{1}H{2}M{3}S", days);
			}
			return (T)str;
		}

		public static ScheduledTaskTrigger Daily(DateTimeOffset startDate, short recureDays)
		{
			return new ScheduledTaskTrigger.DailyTrigger(startDate, recureDays, null);
		}

		public static ScheduledTaskTrigger OneTime(DateTimeOffset when)
		{
			return new ScheduledTaskTrigger.OneTimeTrigger(when);
		}

		public static ScheduledTaskTrigger Weekly(DateTimeOffset startDate, short weeksInterval)
		{
			return new ScheduledTaskTrigger.WeeklyTrigger(startDate, weeksInterval, null, new DayOfWeek[0]);
		}

		private class DailyTrigger : ScheduledTaskTrigger
		{
			private readonly short _recureDays;

			public DailyTrigger(DateTimeOffset startDate, short recureDays, TimeSpan? interval = null) : base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY, interval)
			{
				this._recureDays = recureDays;
			}

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				base.Create<IDailyTrigger>(taskDefinition).DaysInterval = this._recureDays;
			}
		}

		private class OneTimeTrigger : ScheduledTaskTrigger
		{
			public OneTimeTrigger(DateTimeOffset when) : base(when, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME, null)
			{
			}

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				base.Create<ITimeTrigger>(taskDefinition);
			}
		}

		private class WeeklyTrigger : ScheduledTaskTrigger
		{
			private readonly short _weeksInterval;

			private readonly DayOfWeek[] _recurDays;

			private readonly Dictionary<DayOfWeek, short> _bitsForWeekDays;

			public WeeklyTrigger(DateTimeOffset startDate, short weeksInterval, TimeSpan? interval = null, params DayOfWeek[] recurDays) : base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_WEEKLY, interval)
			{
				this._weeksInterval = weeksInterval;
				this._recurDays = recurDays;
				short num1 = 1;
				this._bitsForWeekDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select<DayOfWeek, KeyValuePair<DayOfWeek, short>>((DayOfWeek x) => {
					short num = num1;
					num1 = (short)(num1 * 2);
					return new KeyValuePair<DayOfWeek, short>(x, num);
				}).ToDictionary<KeyValuePair<DayOfWeek, short>, DayOfWeek, short>((KeyValuePair<DayOfWeek, short> x) => x.Key, (KeyValuePair<DayOfWeek, short> x) => x.Value);
			}

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				IWeeklyTrigger variable = base.Create<IWeeklyTrigger>(taskDefinition);
				variable.WeeksInterval = this._weeksInterval;
				variable.DaysOfWeek = (
					from x in this._recurDays.Distinct<DayOfWeek>()
					select this._bitsForWeekDays[x]).Aggregate<short>((short x, short y) => (short)(x | y));
			}
		}
	}
}