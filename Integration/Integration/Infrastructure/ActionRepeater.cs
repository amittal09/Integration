using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Infrastructure
{
	public class ActionRepeater : IDisposable
	{
		private readonly TimeSpan _delay;

		private readonly TextWriter _outputter;

		private Task _task;

		private bool _repeating = true;

		private ActionRepeater(TimeSpan delay, TextWriter outputter)
		{
			this._delay = delay;
			this._outputter = outputter ?? TextWriter.Null;
		}

		public void Dispose()
		{
			this._repeating = false;
			this._outputter.WriteLine("Waiting for Repeater to stop.");
			Task.WaitAll(new Task[] { this._task });
			this._task.Dispose();
			this._task = null;
		}

		private async void Repeat(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			int num = 0;
			this._outputter.WriteLine("Starting Repeater (delay: {0})", this._delay);
			this._task = Task.Factory.StartNew(() => {
				while (this._repeating)
				{
					action();
					num++;
					Thread.Sleep(this._delay);
				}
			}, TaskCreationOptions.LongRunning);
			await this._task;
			this._outputter.WriteLine("Repeater stopped after {0} iterations.", num);
		}

		public static ActionRepeater Start(Action task, TimeSpan delay, TextWriter outputter)
		{
			ActionRepeater actionRepeater = new ActionRepeater(delay, outputter);
			actionRepeater.Repeat(task);
			return actionRepeater;
		}
	}
}