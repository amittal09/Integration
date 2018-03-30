using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration
{
	public class ApplicationEnvironment : IEquatable<ApplicationEnvironment>
	{
		public readonly static ApplicationEnvironment Development;

		public readonly static ApplicationEnvironment Stage;

		public readonly static ApplicationEnvironment Production;

		[Obsolete("Renamed to Stage.")]
		public readonly static ApplicationEnvironment Staging;

		public string Name
		{
			get;
		}

		static ApplicationEnvironment()
		{
			ApplicationEnvironment.Development = new ApplicationEnvironment("Development");
			ApplicationEnvironment.Stage = new ApplicationEnvironment("Stage");
			ApplicationEnvironment.Production = new ApplicationEnvironment("Production");
			ApplicationEnvironment.Staging = new ApplicationEnvironment("Staging");
		}

		protected ApplicationEnvironment(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this.Name = name;
		}

		public static ApplicationEnvironment Custom(string name)
		{
			ApplicationEnvironment applicationEnvironment = new ApplicationEnvironment(name);
			if (applicationEnvironment.Equals(ApplicationEnvironment.Development))
			{
				return ApplicationEnvironment.Development;
			}
			if (applicationEnvironment.Equals(ApplicationEnvironment.Stage))
			{
				return ApplicationEnvironment.Stage;
			}
			if (applicationEnvironment.Equals(ApplicationEnvironment.Staging))
			{
				return ApplicationEnvironment.Staging;
			}
			if (applicationEnvironment.Equals(ApplicationEnvironment.Production))
			{
				return ApplicationEnvironment.Production;
			}
			return applicationEnvironment;
		}

		public bool Equals(ApplicationEnvironment other)
		{
			if (other == null)
			{
				return false;
			}
			if ((object)this == (object)other)
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
			if (!(obj is ApplicationEnvironment))
			{
				return false;
			}
			return this.Equals((ApplicationEnvironment)obj);
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public static bool operator ==(ApplicationEnvironment a, ApplicationEnvironment b)
		{
			return object.Equals(a, b);
		}

		public static implicit operator String(ApplicationEnvironment target)
		{
			if (target == null)
			{
				return null;
			}
			return target.ToString();
		}

		public static implicit operator ApplicationEnvironment(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			return ApplicationEnvironment.Custom(name);
		}

		public static bool operator !=(ApplicationEnvironment a, ApplicationEnvironment b)
		{
			return !object.Equals(a, b);
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}