using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
	public static class FtpClientExtensions
	{
		public static string DownloadToLocal(this IFtpClient client, string name, DirectoryInfo localDirectory)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			if (localDirectory == null)
			{
				throw new ArgumentNullException("localDirectory");
			}
			return client.Download(name, (Stream stream) => {
				using (FileStream fileStream = new FileStream(Path.Combine(localDirectory.FullName, name), FileMode.Create))
				{
					stream.CopyTo(fileStream);
				}
			});
		}

		public static string DownloadToMemoryStream(this IFtpClient client, string name, MemoryStream memoryStream)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			if (memoryStream == null)
			{
				throw new ArgumentNullException("memoryStream");
			}
			return client.Download(name, (Stream stream) => {
				stream.CopyTo(memoryStream);
				memoryStream.Position = (long)0;
			});
		}

		public static string DownloadToString(this IFtpClient client, string name)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			string end = null;
			client.Download(name, (Stream stream) => {
				using (StreamReader streamReader = new StreamReader(stream))
				{
					end = streamReader.ReadToEnd();
				}
			});
			return end;
		}

		public static string UploadFromLocal(this IFtpClient client, FileInfo localFile)
		{
			string str;
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			if (localFile == null)
			{
				throw new ArgumentNullException("localFile");
			}
			using (FileStream fileStream = localFile.OpenRead())
			{
				str = client.Upload(localFile.Name, fileStream, true);
			}
			return str;
		}

		public static string UploadFromString(this IFtpClient client, string name, string content, Encoding encoding = null)
		{
			string str;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (StreamWriter streamWriter = (encoding == null ? new StreamWriter(memoryStream) : new StreamWriter(memoryStream, encoding)))
				{
					streamWriter.Write(content ?? string.Empty);
					streamWriter.Flush();
					memoryStream.Position = (long)0;
					str = client.Upload(name, memoryStream, false);
				}
			}
			return str;
		}
	}
}