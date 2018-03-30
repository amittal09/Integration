using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvRow : DynamicObject, IEnumerable<string>, IEnumerable
	{
		private readonly string[] _data;

		private readonly IDictionary<string, int> _headers;

		public bool IsEmpty
		{
			get
			{
				return this._data.All<string>(new Func<string, bool>(string.IsNullOrEmpty));
			}
		}

		public string this[string name]
		{
			get
			{
				return this._data[this.GetIndexByName(name)];
			}
			set
			{
				this._data[this.GetIndexByName(name)] = value;
			}
		}

		public string this[int index]
		{
			get
			{
				if (index < 0 || index >= (int)this._data.Length)
				{
					throw new IndexOutOfRangeException();
				}
				return this._data[index];
			}
			set
			{
				if (index < 0 || index >= (int)this._data.Length)
				{
					throw new IndexOutOfRangeException();
				}
				this._data[index] = value;
			}
		}

		public int Length
		{
			get
			{
				return (int)this._data.Length;
			}
		}

		public CsvRow.CsvRowMetadata Meta
		{
			get;
		}

		public CsvRow(string[] data, string delimiter = ";", IDictionary<string, int> headers = null, uint? lineNumber = null)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (headers != null && headers.Count > 0 && (int)data.Length != headers.Count)
			{
				throw new ArgumentException(string.Format("Row{0} has {1} columns but we expected {2} columns (equal to number of header columns).", (lineNumber.HasValue ? string.Concat(" #", lineNumber.Value) : string.Empty), (int)data.Length, headers.Count));
			}
			this._data = data;
			this._headers = headers;
			this.Meta = new CsvRow.CsvRowMetadata(this, delimiter, lineNumber);
		}

		public static CsvRow.ICsvRowBuilder BeginRows(params string[] headers)
		{
			return new CsvRow.CsvRowBuilder(headers);
		}

		internal string Escape(string data)
		{
			data = (new StringBuilder(data)).Replace("\"", "\"\"").Replace("“", "\"\"").Replace("”", "\"\"").Replace("„", "\"\"").ToString();
			if (data.IndexOf(this.Meta.Delimiter, StringComparison.OrdinalIgnoreCase) >= 0 || data.Contains<char>('\"') || data.Contains(Environment.NewLine))
			{
				data = string.Format("\"{0}\"", data);
			}
			return data;
		}

		public static CsvRow[] From<T>(IEnumerable<T> elements, Action<CsvRow.ICsvRowBuilderConfiguration> configure = null)
		{
			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}
			PropertyInfo[] array = (
				from  x in (IEnumerable<PropertyInfo>)typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where x.CanRead
				select x).ToArray<PropertyInfo>();
			CsvRow.ICsvRowBuilder csvRowBuilder = CsvRow.BeginRows((
				from x in (IEnumerable<PropertyInfo>)array
				select x.Name).ToArray<string>());
			if (configure != null)
			{
				csvRowBuilder.Configure(configure);
			}
			foreach (T element in elements)
			{
				csvRowBuilder.Add((
					from x in array
					select x.GetValue(element)).Select<object, string>((object x) => {
					if (x == null)
					{
						return null;
					}
					return x.ToString();
				}).ToArray<string>());
			}
			return csvRowBuilder.End().ToRows();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return this._data.Select<string, string>(new Func<string, string>(this.Escape)).GetEnumerator();
		}

		private int GetIndexByName(string name)
		{
			int num;
			if (this._headers == null)
			{
				throw new InvalidOperationException(string.Format("Row was not initialized with headers.", new object[0]));
			}
			if (!this._headers.TryGetValue(name, out num))
			{
				throw new ArgumentException(string.Format("Could not find any header named '{0}'.", name));
			}
			return num;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public override string ToString()
		{
			return string.Join(this.Meta.Delimiter, this);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			string str;
			string name = binder.Name;
			if (value != null)
			{
				str = value.ToString();
			}
			else
			{
				str = null;
			}
			this[name] = str;
			return true;
		}

		public class CsvRowBuilder : CsvRow.ICsvRowBuilder, CsvRow.ICsvRowBuilderAdder, CsvRow.ICsvRowBuilderFinisher, CsvRow.ICsvRowBuilderConfiguration
		{
			private IDictionary<string, int> _headers;

			private readonly List<CsvRow> _rows;

			private string _delimiter;

			private bool _headerInserted;

			private bool _returnHeaderAsRow;

			public int DataRowCount
			{
				get
				{
					int count = this._rows.Count;
					if (!this._headerInserted)
					{
						return count;
					}
					return count - 1;
				}
			}

			internal CsvRowBuilder(string[] headers)
			{
				this.Headers(headers);
				this._rows = new List<CsvRow>();
				this._delimiter = ";";
			}

			public CsvRow.ICsvRowBuilderFinisher Add(params string[] data)
			{
				if (data == null)
				{
					throw new ArgumentNullException("data");
				}
				int count = this._rows.Count + 1 + (this._returnHeaderAsRow ? 1 : 0);
				this._rows.Add(new CsvRow(data, this._delimiter, this._headers, new uint?((uint)count)));
				return this;
			}

			public CsvRow.ICsvRowBuilderFinisher AddUsingMapper(Action<CsvRow.ICsvRowMapper> mapper)
			{
				if (mapper == null)
				{
					throw new ArgumentNullException("mapper");
				}
				if (this._headers == null)
				{
					throw new InvalidOperationException("No headers were passed so this method is not allowed.");
				}
				CsvRow.CsvRowMapper csvRowMapper = new CsvRow.CsvRowMapper(this._headers);
				mapper(csvRowMapper);
				this.Add(csvRowMapper.ToData());
				return this;
			}

			public CsvRow.ICsvRowBuilderConfiguration ChangeDelimiter(string delimiter)
			{
				this._delimiter = delimiter ?? string.Empty;
				return this;
			}

			public CsvRow.ICsvRowBuilderAdder Configure(Action<CsvRow.ICsvRowBuilderConfiguration> configure)
			{
				if (configure != null)
				{
					configure(this);
				}
				return this;
			}

			public CsvRow.ICsvRowBuilderFinisher End()
			{
				return this;
			}

			public CsvRow.ICsvRowBuilderFinisher From<T>(IEnumerable<T> elements, Func<T, string[]> createData)
			{
				if (elements == null)
				{
					throw new ArgumentNullException("elements");
				}
				if (createData == null)
				{
					throw new ArgumentNullException("createData");
				}
				elements.ForEach<T>((T x) => this.Add(createData(x)));
				return this;
			}

			public CsvRow.ICsvRowBuilderFinisher FromUsingMapper<T>(IEnumerable<T> elements, Action<CsvRow.ICsvRowMapper, T> mapper)
			{
				if (elements == null)
				{
					throw new ArgumentNullException("elements");
				}
				if (mapper == null)
				{
					throw new ArgumentNullException("mapper");
				}
				if (this._headers == null)
				{
					throw new InvalidOperationException("No headers were passed so this method is not allowed.");
				}
				elements.ForEach<T>((T x) => {
					CsvRow.CsvRowMapper csvRowMapper = new CsvRow.CsvRowMapper(this._headers);
					mapper(csvRowMapper, x);
					this.Add(csvRowMapper.ToData());
				});
				return this;
			}

			public CsvRow.ICsvRowBuilderFinisher Headers(params string[] headers)
			{
				if (this._headers != null)
				{
					throw new InvalidOperationException("Headers have already been set.");
				}
				if (headers != null && headers.Length != 0)
				{
					this._headers = headers.Select((string x, int i) => new { Header = x, Index = i }).ToDictionary((x) => x.Header, (x) => x.Index, StringComparer.OrdinalIgnoreCase);
				}
				return this;
			}

			public static implicit operator CsvRow[](CsvRow.CsvRowBuilder builder)
			{
				if (builder == null)
				{
					throw new ArgumentNullException("builder");
				}
				return builder.ToRows();
			}

			public CsvRow.ICsvRowBuilderConfiguration ReturnHeaderAsRow()
			{
				this._returnHeaderAsRow = true;
				return this;
			}

			public CsvRow[] ToRows()
			{
				if (this._headers != null && this._returnHeaderAsRow && !this._headerInserted)
				{
					this._rows.Insert(0, new CsvRow(this._headers.Keys.ToArray<string>(), this._delimiter, this._headers, new uint?(1)));
					this._headerInserted = true;
				}
				return this._rows.ToArray();
			}

			public override string ToString()
			{
				return string.Join(Environment.NewLine, 
					from x in this.ToRows()
					select x.ToString());
			}
		}

		private class CsvRowMapper : CsvRow.ICsvRowMapper
		{
			private readonly Dictionary<string, string> _data;

			private readonly IDictionary<string, int> _headers;

			public CsvRowMapper(IDictionary<string, int> headers)
			{
				if (headers == null)
				{
					throw new ArgumentNullException("headers");
				}
				this._headers = headers;
				this._data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (string key in this._headers.Keys)
				{
					this._data[key] = string.Empty;
				}
			}

			public CsvRow.ICsvRowMapper Map(string name, string value)
			{
				if (!this._data.ContainsKey(name))
				{
					throw new KeyNotFoundException(string.Format("Could not find any header named '{0}'.", name));
				}
				this._data[name] = value;
				return this;
			}

			public string[] ToData()
			{
				return (
					from x in this._headers
					orderby x.Value
					select this._data[x.Key]).ToArray<string>();
			}
		}

		public class CsvRowMetadata
		{
			private readonly CsvRow _row;

			public string Delimiter
			{
				get;
			}

			public CsvRow.CsvRowMetadata.CsvRowHeaders Headers
			{
				get;
				private set;
			}

			public uint? LineNumber
			{
				get;
				private set;
			}

			internal CsvRowMetadata(CsvRow row, string delimiter, uint? lineNumber = null)
			{
				if (row == null)
				{
					throw new ArgumentNullException("row");
				}
				this._row = row;
				if (this._row._headers != null)
				{
					this.Headers = new CsvRow.CsvRowMetadata.CsvRowHeaders(this);
				}
				this.Delimiter = delimiter ?? string.Empty;
				this.LineNumber = lineNumber;
			}

			public class CsvRowHeaders : IEnumerable<string>, IEnumerable
			{
				private readonly CsvRow.CsvRowMetadata _metadata;

				public int Length
				{
					get
					{
						return this._metadata._row._headers.Count;
					}
				}

				internal CsvRowHeaders(CsvRow.CsvRowMetadata metadata)
				{
					if (metadata == null)
					{
						throw new ArgumentNullException("metadata");
					}
					if (metadata._row._headers == null)
					{
						throw new ArgumentException("No headers present.", "metadata");
					}
					this._metadata = metadata;
				}

				public IEnumerator<string> GetEnumerator()
				{
					return (
						from x in this._metadata._row._headers.Keys
						select this._metadata._row.Escape(x)).GetEnumerator();
				}

				public static implicit operator String[](CsvRow.CsvRowMetadata.CsvRowHeaders headers)
				{
					if (headers == null)
					{
						throw new ArgumentNullException("headers");
					}
					return headers._metadata._row._headers.Keys.ToArray<string>();
				}

				IEnumerator System.Collections.IEnumerable.GetEnumerator()
				{
					return this.GetEnumerator();
				}

				public override string ToString()
				{
					return string.Join(this._metadata.Delimiter, this);
				}
			}
		}

		public interface ICsvRowBuilder : CsvRow.ICsvRowBuilderAdder
		{
			CsvRow.ICsvRowBuilderAdder Configure(Action<CsvRow.ICsvRowBuilderConfiguration> configure);
		}

		public interface ICsvRowBuilderAdder
		{
			int DataRowCount
			{
				get;
			}

			CsvRow.ICsvRowBuilderFinisher Add(params string[] data);

			CsvRow.ICsvRowBuilderFinisher AddUsingMapper(Action<CsvRow.ICsvRowMapper> mapper);

			CsvRow.ICsvRowBuilderFinisher End();

			CsvRow.ICsvRowBuilderFinisher From<T>(IEnumerable<T> elements, Func<T, string[]> createData);

			CsvRow.ICsvRowBuilderFinisher FromUsingMapper<T>(IEnumerable<T> elements, Action<CsvRow.ICsvRowMapper, T> mapper);

			CsvRow.ICsvRowBuilderFinisher Headers(params string[] headers);
		}

		public interface ICsvRowBuilderConfiguration
		{
			CsvRow.ICsvRowBuilderConfiguration ChangeDelimiter(string delimiter);

			CsvRow.ICsvRowBuilderConfiguration ReturnHeaderAsRow();
		}

		public interface ICsvRowBuilderFinisher : CsvRow.ICsvRowBuilderAdder
		{
			CsvRow[] ToRows();
		}

		public interface ICsvRowMapper
		{
			CsvRow.ICsvRowMapper Map(string name, string value);
		}
	}
}