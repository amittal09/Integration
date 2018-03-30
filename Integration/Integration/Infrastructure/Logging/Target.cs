using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class Target : ITarget, IEquatable<Target>
	{
		public readonly static Target Service;

		public readonly static Target All;

		public string Name
		{
			get;
		}

		static Target()
		{
			Target.Service = new Target("Service");
			Target.All = new Target("All");
		}

		protected Target(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this.Name = name;
		}

		public static Target Custom(string name)
		{
			Target target = new Target(name);
			if (target.Equals(Target.Service))
			{
				return Target.Service;
			}
			if (target.Equals(Target.All))
			{
				return Target.All;
			}
			return target;
		}

		public bool Equals(Target other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
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
			if (!(obj is Target))
			{
				return false;
			}
			return this.Equals((Target)obj);
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public static implicit operator String(Target target)
		{
			if (target == null)
			{
				return null;
			}
			return target.ToString();
		}

		public static implicit operator Target(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			return Target.Custom(name);
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}