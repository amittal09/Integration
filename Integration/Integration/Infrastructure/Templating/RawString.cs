using System;
using System.Net;
using System.Web;

namespace Vertica.Integration.Infrastructure.Templating
{
	public class RawString : IHtmlString
	{
		private readonly string _text;

		public RawString(string text)
		{
			this._text = text;
		}

		public string ToHtmlString()
		{
			return WebUtility.HtmlEncode(this._text);
		}

		public override string ToString()
		{
			return this._text;
		}
	}
}