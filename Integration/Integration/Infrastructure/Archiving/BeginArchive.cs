using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
	public class BeginArchive : IDisposable
	{
		private readonly Action<MemoryStream, ArchiveOptions> _complete;

		private readonly MemoryStream _stream;

		private readonly ZipArchive _archive;

		public ArchiveOptions Options
		{
			get;
		}

		public BeginArchive(string name, Action<MemoryStream, ArchiveOptions> complete)
		{
			if (complete == null)
			{
				throw new ArgumentNullException("complete");
			}
			this._complete = complete;
			this.Options = new ArchiveOptions(name);
			this._stream = new MemoryStream();
			this._archive = new ZipArchive(this._stream, ZipArchiveMode.Create, true);
		}

		private BeginArchive CreateEntryFromFile(FileInfo file, string relativePath = null)
		{
			this._archive.CreateEntryFromFile(file.FullName, Path.Combine(relativePath ?? string.Empty, file.Name));
			return this;
		}

		public void Dispose()
		{
			this._archive.Dispose();
			this._stream.Seek((long)0, SeekOrigin.Begin);
			this._complete(this._stream, this.Options);
			this._stream.Dispose();
		}

		public BeginArchive IncludeBinary(string fileName, byte[] content)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "fileName");
			}
			using (BinaryWriter binaryWriter = new BinaryWriter(this._archive.CreateEntry(fileName).Open()))
			{
				binaryWriter.Write(content);
			}
			return this;
		}

		public BeginArchive IncludeContent(string name, string content, string extension = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			name = Regex.Replace(name, "[^\\w\\s\\.]", string.Empty);
			extension = extension.NullIfEmpty() ?? (Path.GetExtension(name).NullIfEmpty() ?? ".txt");
			name = Path.GetFileNameWithoutExtension(name);
			using (StreamWriter streamWriter = new StreamWriter(this._archive.CreateEntry(string.Format("{0}{1}", name, extension)).Open()))
			{
				streamWriter.Write(content);
			}
			return this;
		}

		public BeginArchive IncludeFile(FileInfo file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			if (!file.Exists)
			{
				throw new ArgumentException(string.Format("File '{0}' does not exist.", file.FullName));
			}
			return this.CreateEntryFromFile(file, null);
		}

		public BeginArchive IncludeFolder(DirectoryInfo folder)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (!folder.Exists)
			{
				throw new ArgumentException(string.Format("File '{0}' does not exist.", folder.FullName));
			}
			Stack<string> strs = new Stack<string>();
			strs.Push(folder.Name);
			return this.IncludeFolderRecursive(folder, strs);
		}

		private BeginArchive IncludeFolderRecursive(DirectoryInfo folder, Stack<string> path)
		{
			foreach (FileInfo fileInfo in folder.EnumerateFiles())
			{
				this.CreateEntryFromFile(fileInfo, Path.Combine(path.Reverse<string>().ToArray<string>()));
			}
			foreach (DirectoryInfo directoryInfo in folder.EnumerateDirectories())
			{
				path.Push(directoryInfo.Name);
				this.IncludeFolderRecursive(directoryInfo, path);
				path.Pop();
			}
			return this;
		}

		public BeginArchive IncludeObjectAsJson(object obj, string fileNameWithoutExtension = null)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			string str = JsonConvert.SerializeObject(obj,Formatting.Indented);
			this.IncludeContent(fileNameWithoutExtension ?? obj.GetType().Name, str, ".json");
			return this;
		}
	}
}