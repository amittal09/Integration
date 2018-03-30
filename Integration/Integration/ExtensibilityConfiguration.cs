using System;
using System.Collections;
using System.Collections.Generic;

namespace Vertica.Integration
{
	public class ExtensibilityConfiguration : IEnumerable<object>, IEnumerable
	{
		private readonly Dictionary<Type, object> _extensions;

		internal ExtensibilityConfiguration()
		{
			this._extensions = new Dictionary<Type, object>();
		}

		internal T Get<T>()
		where T : class
		{
			return (T)(this._extensions[typeof(T)] as T);
		}

		public IEnumerator<object> GetEnumerator()
		{
			return this._extensions.Values.GetEnumerator();
		}

		public T Register<T>(Func<T> factory)
		where T : class
		{
			T t;
			object obj;
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			if (!this._extensions.TryGetValue(typeof(T), out obj))
			{
				Dictionary<Type, object> types = this._extensions;
				Type type = typeof(T);
				T t1 = factory();
				t = t1;
				types[type] = t1;
			}
			else
			{
				t = (T)obj;
			}
			return t;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}