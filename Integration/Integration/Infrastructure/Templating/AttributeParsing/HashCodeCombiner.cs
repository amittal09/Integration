using System;
using System.Collections;

namespace Vertica.Integration.Infrastructure.Templating.AttributeParsing
{
	internal class HashCodeCombiner
	{
		private long _combinedHash64 = (long)5381;

		public int CombinedHash
		{
			get
			{
				return this._combinedHash64.GetHashCode();
			}
		}

		public HashCodeCombiner()
		{
		}

		public HashCodeCombiner Add(IEnumerable e)
		{
			if (e != null)
			{
				int num = 0;
				foreach (object obj in e)
				{
					this.Add(obj);
					num++;
				}
				this.Add(num);
			}
			else
			{
				this.Add(0);
			}
			return this;
		}

		public HashCodeCombiner Add(int i)
		{
			this._combinedHash64 = (this._combinedHash64 << 5) + this._combinedHash64 ^ (long)i;
			return this;
		}

		public HashCodeCombiner Add(object o)
		{
			this.Add((o != null ? o.GetHashCode() : 0));
			return this;
		}

		public static HashCodeCombiner Start()
		{
			return new HashCodeCombiner();
		}
	}
}