using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Templating.AttributeParsing
{
	[DebuggerDisplay("({Position})\"{Value}\"")]
	public class PositionTagged<T>
	{
		public int Position
		{
			get;
		}

		public T Value
		{
			get;
		}

		public PositionTagged(T value, int offset)
		{
			this.Position = offset;
			this.Value = value;
		}

		public override bool Equals(object obj)
		{
			PositionTagged<T> positionTagged = obj as PositionTagged<T>;
			if (!(positionTagged != null) || positionTagged.Position != this.Position)
			{
				return false;
			}
			return object.Equals(positionTagged.Value, this.Value);
		}

		public override int GetHashCode()
		{
			return HashCodeCombiner.Start().Add(this.Position).Add(this.Value).CombinedHash;
		}

		public static bool operator ==(PositionTagged<T> left, PositionTagged<T> right)
		{
			return object.Equals(left, right);
		}

		public static implicit operator T(PositionTagged<T> value)
		{
			return value.Value;
		}

		public static implicit operator PositionTagged<T>(Tuple<T, int> value)
		{
			return new PositionTagged<T>(value.Item1, value.Item2);
		}

		public static bool operator !=(PositionTagged<T> left, PositionTagged<T> right)
		{
			return !object.Equals(left, right);
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}
	}
}