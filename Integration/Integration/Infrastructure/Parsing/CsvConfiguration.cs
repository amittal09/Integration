using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvConfiguration
	{
		public const string DefaultDelimiter = ";";

		internal string Delimiter
		{
			get;
			private set;
		}

		internal System.Text.Encoding Encoding
		{
			get;
			private set;
		}

		internal bool FirstLineIsHeader
		{
			get;
			private set;
		}

		public CsvConfiguration()
		{
			this.FirstLineIsHeader = true;
			this.Encoding = System.Text.Encoding.UTF8;
			this.Delimiter = ";";
		}

		public CsvConfiguration ChangeDelimiter(string delimiter)
		{
			if (string.IsNullOrWhiteSpace(delimiter))
			{
				throw new ArgumentException("Value cannot be null or empty.", "delimiter");
			}
			this.Delimiter = delimiter;
			return this;
		}

		public CsvConfiguration ChangeEncoding(System.Text.Encoding encoding)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException("encoding");
			}
			this.Encoding = encoding;
			return this;
		}

		public CsvConfiguration NoHeaders()
		{
			this.FirstLineIsHeader = false;
			return this;
		}
	}
}