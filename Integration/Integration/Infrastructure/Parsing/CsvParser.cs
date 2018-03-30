using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvParser : ICsvParser
	{
		public CsvParser()
		{
		}

		public IEnumerable<CsvRow> Parse(Stream stream, Action<CsvConfiguration> csv = null)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			CsvConfiguration csvConfiguration = new CsvConfiguration();
			if (csv != null)
			{
				csv(csvConfiguration);
			}
			string[][] array = this.Read(stream, csvConfiguration).ToArray<string[]>();
			Dictionary<string, int> dictionary = null;
			if (csvConfiguration.FirstLineIsHeader && array.Length != 0)
			{
				dictionary = array.First<string[]>().Select((string name, int index) => new { name = name, index = index }).ToDictionary((x) => x.name, (x) => x.index, StringComparer.OrdinalIgnoreCase);
			}
			int num = (dictionary != null ? 2 : 1);
			return array.Skip<string[]>((dictionary != null ? 1 : 0)).Select<string[], CsvRow>((string[] x, int i) => new CsvRow(x, csvConfiguration.Delimiter, dictionary, new uint?((uint)(i + num))));
		}

		private IEnumerable<string[]> Read(Stream stream, CsvConfiguration configuration)
		{
			string[] strArrays = null;
			int? nullable = null;
			using (TextFieldParser textFieldParser = new TextFieldParser(stream, configuration.Encoding))
			{
				textFieldParser.SetDelimiters(new string[] { configuration.Delimiter });
				while (!textFieldParser.EndOfData)
				{
					string[] strArrays1 = textFieldParser.ReadFields();
					if (strArrays1 == null)
					{
						strArrays1 = new string[0];
					}
					string[] array = strArrays1;
					if (!nullable.HasValue)
					{
						nullable = new int?((int)array.Length);
					}
					if (strArrays != null)
					{
						strArrays[(int)strArrays.Length - 1] = string.Format("{0}{1}{2}", strArrays[(int)strArrays.Length - 1], Environment.NewLine, array[0]);
						array = strArrays.Concat<string>(array.Skip<string>(1)).ToArray<string>();
					}
					if ((int)array.Length == nullable.Value)
					{
						strArrays = null;
						yield return array;
					}
					else
					{
						strArrays = array;
					}
				}
			}
			//textFieldParser = null;
		}
	}
}