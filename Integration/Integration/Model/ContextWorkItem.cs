using System;
using System.Collections.Generic;
using System.Reflection;

namespace Vertica.Integration.Model
{
	public abstract class ContextWorkItem
	{
		private readonly IDictionary<string, object> _context;

		public object this[string name]
		{
			get
			{
				object obj;
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", "name");
				}
				this._context.TryGetValue(name, out obj);
				return obj;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("Value cannot be null or empty.", "name");
				}
				this._context[name] = value;
			}
		}

		protected ContextWorkItem()
		{
			this._context = new Dictionary<string, object>();
		}

		public T Context<T>(string name, T context = null)
		where T : class
		{
			if (context == null)
			{
				return (T)(this[name] as T);
			}
			this[name] = context;
			return context;
		}
	}
}