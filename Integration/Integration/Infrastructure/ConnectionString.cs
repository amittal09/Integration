using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure
{
	public sealed class ConnectionString : IEquatable<ConnectionString>
	{
		private readonly Lazy<string> _value;

		public static ConnectionString Empty
		{
			get
			{
				return ConnectionString.FromText(string.Empty);
			}
		}

		private ConnectionString(Func<string> value)
		{
			this._value = new Lazy<string>(value);
		}

		public bool Equals(ConnectionString other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (!(obj is ConnectionString))
			{
				return false;
			}
			return this.Equals((ConnectionString)obj);
		}

		public static ConnectionString FromName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			return new ConnectionString(() => {
				ConnectionStringSettings item = ConfigurationManager.ConnectionStrings[name];
				if (item == null)
				{
					throw new ArgumentException(string.Format("No ConnectionString found with name '{0}'. Please add this to the <connectionString> element.", name));
				}
				return item.ConnectionString;
			});
		}

		public static ConnectionString FromText(string text)
		{
			return new ConnectionString(() => text);
		}

		public override int GetHashCode()
		{
			return this.ToString().ToLowerInvariant().GetHashCode();
		}

		public static implicit operator String(ConnectionString connectionString)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			return connectionString.ToString();
		}

		public override string ToString()
		{
			return this._value.Value ?? string.Empty;
		}
	}
}