using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Templating.AttributeParsing
{
	public class AttributeValue
	{
		public bool Literal
		{
			get;
			private set;
		}

		public PositionTagged<string> Prefix
		{
			get;
			private set;
		}

		public PositionTagged<object> Value
		{
			get;
			private set;
		}

		public AttributeValue(PositionTagged<string> prefix, PositionTagged<object> value, bool literal)
		{
			this.Prefix = prefix;
			this.Value = value;
			this.Literal = literal;
		}

		public static AttributeValue FromTuple(Tuple<Tuple<string, int>, Tuple<object, int>, bool> value)
		{
			return new AttributeValue(value.Item1, value.Item2, value.Item3);
		}

		public static AttributeValue FromTuple(Tuple<Tuple<string, int>, Tuple<string, int>, bool> value)
		{
			return new AttributeValue(value.Item1, new PositionTagged<object>(value.Item2.Item1, value.Item2.Item2), value.Item3);
		}

		public static implicit operator AttributeValue(Tuple<Tuple<string, int>, Tuple<object, int>, bool> value)
		{
			return AttributeValue.FromTuple(value);
		}

		public static implicit operator AttributeValue(Tuple<Tuple<string, int>, Tuple<string, int>, bool> value)
		{
			return AttributeValue.FromTuple(value);
		}
	}
}