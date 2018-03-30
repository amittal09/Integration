using System;
using System.Net;

namespace Vertica.Integration.Infrastructure.Templating
{
	public class HtmlHelper
	{
		public HtmlHelper()
		{
		}

		public string Encode(string value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			return WebUtility.HtmlEncode(value);
		}

		public string Encode(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			return this.Encode(value.ToString());
		}

		public RawString HtmlString(object value)
		{
			if (value == null)
			{
				return null;
			}
			return new RawString(value.ToString());
		}

		public RawString Raw(string html)
		{
			return new RawString(html);
		}
	}
}