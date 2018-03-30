using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
	internal class FtpClient : IFtpClient
	{
		private readonly FtpClientConfiguration _configuration;

		private readonly Stack<Stack<string>> _paths;

		public string CurrentPath
		{
			get
			{
				return string.Concat("/", (this._paths.Count > 0 ? string.Join("/", this._paths.Peek().Reverse<string>().ToArray<string>()) : string.Empty));
			}
		}

		public FtpClient(FtpClientConfiguration configuration)
		{
			this._configuration = configuration;
			this._paths = new Stack<Stack<string>>();
		}

		public string CreateDirectory(string name)
		{
			string str;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				str = this.WithExceptionHandling<string>(() => {
					string currentPath;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.Method = "MKD";
					using (WebResponse response = ftpWebRequest.GetResponse())
					{
						currentPath = this.CurrentPath;
					}
					return currentPath;
				});
			}
			return str;
		}

		public string CreateDirectoryAndEnterIt(string name)
		{
			return this.NavigateTo(this.CreateDirectory(name));
		}

		private FtpWebRequest CreateRequest()
		{
			return this._configuration.CreateRequest(this.CurrentPath);
		}

		public string DeleteDirectory(string name)
		{
			string str;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				str = this.WithExceptionHandling<string>(() => {
					string currentPath;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.Method = "RMD";
					using (WebResponse response = ftpWebRequest.GetResponse())
					{
						currentPath = this.CurrentPath;
					}
					return currentPath;
				});
			}
			return str;
		}

		public string DeleteFile(string name)
		{
			string str;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				str = this.WithExceptionHandling<string>(() => {
					string currentPath;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.Method = "DELE";
					using (WebResponse response = ftpWebRequest.GetResponse())
					{
						currentPath = this.CurrentPath;
					}
					return currentPath;
				});
			}
			return str;
		}

		public string Download(string name, Action<Stream> data)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.NavigateDown(name);
			return this.WithExceptionHandling<string>(() => {
				string currentPath;
				FtpWebRequest ftpWebRequest = this.CreateRequest();
				ftpWebRequest.Method = "RETR";
				using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream() ?? Stream.Null)
					{
						this.NavigateBack();
						data(responseStream);
						currentPath = this.CurrentPath;
					}
				}
				return currentPath;
			});
		}

		private IDisposable Enter(string name)
		{
			this.NavigateDown(name);
			return new DisposableAction(() => this.NavigateBack());
		}

		public DateTime GetFileLastModified(string name)
		{
			DateTime dateTime;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				dateTime = this.WithExceptionHandling<DateTime>(() => {
					DateTime lastModified;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.Method = "MDTM";
					using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse())
					{
						lastModified = response.LastModified;
					}
					return lastModified;
				});
			}
			return dateTime;
		}

		public long GetFileSize(string name)
		{
			long num;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				num = this.WithExceptionHandling<long>(() => {
					long contentLength;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.Method = "SIZE";
					using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse())
					{
						contentLength = response.ContentLength;
					}
					return contentLength;
				});
			}
			return num;
		}

		public string[] ListDirectory(Func<string, bool> predicate = null)
		{
			return this.WithExceptionHandling<string[]>(() => {
				List<string> strs = new List<string>();
				FtpWebRequest ftpWebRequest = this.CreateRequest();
				ftpWebRequest.Method = "NLST";
				using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream())
					{
						using (StreamReader streamReader = new StreamReader(responseStream ?? Stream.Null))
						{
							while (!streamReader.EndOfStream)
							{
								string str = streamReader.ReadLine() ?? string.Empty;
								if (predicate != null && !predicate(str))
								{
									continue;
								}
								strs.Add(str);
							}
						}
					}
				}
				return strs.ToArray();
			});
		}

		public string NavigateBack()
		{
			if (this._paths.Count > 0)
			{
				Stack<string> strs = this._paths.Peek();
				if (strs.Count <= 1)
				{
					this._paths.Pop();
				}
				else
				{
					strs.Pop();
				}
			}
			return this.CurrentPath;
		}

		public string NavigateDown(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			name = name.TrimStart(new char[] { '/' });
			if (this._paths.Count == 0)
			{
				return this.NavigateTo(name);
			}
			this._paths.Peek().Push(name);
			return this.CurrentPath;
		}

		public string NavigateTo(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("Value cannot be null or empty.", "path");
			}
			path = path.TrimStart(new char[] { '/' });
			this._paths.Push(new Stack<string>(new string[] { path }));
			return this.CurrentPath;
		}

		public string Upload(string name, Stream data, bool binary = true)
		{
			string str;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			using (IDisposable disposable = this.Enter(name))
			{
				str = this.WithExceptionHandling<string>(() => {
					string currentPath;
					FtpWebRequest ftpWebRequest = this.CreateRequest();
					ftpWebRequest.UseBinary = binary;
					ftpWebRequest.Method = "STOR";
					using (Stream requestStream = ftpWebRequest.GetRequestStream() ?? Stream.Null)
					{
						data.CopyTo(requestStream);
						currentPath = this.CurrentPath;
					}
					return currentPath;
				});
			}
			return str;
		}

		private T WithExceptionHandling<T>(Func<T> action)
		{
			T t;
			try
			{
				t = action();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MethodBase method = (new StackFrame(1)).GetMethod();
				throw new FtpClientException(string.Format("{0}.{1}", (method.DeclaringType != null ? method.DeclaringType.Name : "n/a"), method.Name), this.CurrentPath, exception);
			}
			return t;
		}
	}
}