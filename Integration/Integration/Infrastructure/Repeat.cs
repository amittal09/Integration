using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vertica.Integration.Infrastructure
{
	public struct Repeat : IEnumerable<int>, IEnumerable
	{
		private readonly int _times;

		public readonly static Repeat Twice;

		static Repeat()
		{
			Repeat.Twice = (Repeat)2;
		}

		private Repeat(int times)
		{
			this._times = times;
		}

		public IEnumerator<int> GetEnumerator()
		{
			return Enumerable.Range(1, this._times).GetEnumerator();
		}

		public static explicit operator Repeat(uint times)
		{
			return new Repeat((int)times);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public static Repeat Times(uint value)
		{
			return (Repeat)value;
		}

		public override string ToString()
		{
			return string.Format("{0} time{1}", this._times, (this._times == 1 ? string.Empty : "s"));
		}
	}
}