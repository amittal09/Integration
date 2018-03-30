using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4.Comparisons;

namespace Vertica.Integration.Model
{
	public class Arguments : IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		private readonly string _prefix;

		private readonly KeyValuePair<string, string>[] _pairs;

		private readonly Dictionary<string, string> _dictionary;

		public static Arguments Empty
		{
			get
			{
				return new Arguments(new string[0]);
			}
		}

		public string this[int index]
		{
			get
			{
				return this._pairs[index].Key;
			}
		}

		public string this[string key]
		{
			get
			{
				string str;
				this.TryGetValue(key, out str);
				return str;
			}
		}

		public int Length
		{
			get
			{
				return (int)this._pairs.Length;
			}
		}

		public Arguments() : this(new string[0])
		{
		}

		public Arguments(params string[] values) : this((values ?? new string[0]).Select<string, KeyValuePair<string, string>>((string x) => new KeyValuePair<string, string>(x, x)).ToArray<KeyValuePair<string, string>>())
		{
		}

		public Arguments(params KeyValuePair<string, string>[] pairs) : this(null, pairs)
		{
		}

		internal Arguments(string prefix, params KeyValuePair<string, string>[] pairs)
		{
			this._prefix = prefix ?? string.Empty;
			ChainableEqualizer<KeyValuePair<string, string>> chainableEqualizer = Eq<KeyValuePair<string, string>>.By((KeyValuePair<string, string> x, KeyValuePair<string, string> y) => string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase), (KeyValuePair<string, string> x) => x.Key.ToLowerInvariant().GetHashCode());
			this._pairs = ((IEnumerable<KeyValuePair<string, string>>)(pairs ?? new KeyValuePair<string, string>[0])).Distinct<KeyValuePair<string, string>>(chainableEqualizer).ToArray<KeyValuePair<string, string>>();
			this._dictionary = ((IEnumerable<KeyValuePair<string, string>>)this._pairs).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value, StringComparer.OrdinalIgnoreCase);
		}

		public bool Contains(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Value cannot be null or empty.", "key");
			}
			return this._dictionary.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this._pairs.OfType<KeyValuePair<string, string>>().GetEnumerator();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public override string ToString()
		{
			return string.Join(" ", 
				from x in this._pairs
				select string.Format("{0}{1}{2}", this._prefix, x.Key, (string.IsNullOrWhiteSpace(x.Value) || string.Equals(x.Key, x.Value) ? string.Empty : string.Concat(":", x.Value))));
		}

		public bool TryGetValue(string key, out string value)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Value cannot be null or empty.", "key");
			}
			return this._dictionary.TryGetValue(key, out value);
		}
	}
}