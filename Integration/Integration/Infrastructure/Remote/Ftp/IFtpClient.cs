using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
	public interface IFtpClient
	{
		string CurrentPath
		{
			get;
		}

		string CreateDirectory(string name);

		string CreateDirectoryAndEnterIt(string name);

		string DeleteDirectory(string name);

		string DeleteFile(string name);

		string Download(string name, Action<Stream> data);

		DateTime GetFileLastModified(string name);

		long GetFileSize(string name);

		string[] ListDirectory(Func<string, bool> predicate = null);

		string NavigateBack();

		string NavigateDown(string name);

		string NavigateTo(string path);

		string Upload(string name, Stream data, bool binary = true);
	}
}