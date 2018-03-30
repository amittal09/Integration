using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
	public class FtpClientConfiguration
	{
		private readonly Func<string, FtpWebRequest> _request;

		private NetworkCredential _credentials;

		internal FtpClientConfiguration(string uri)
		{
			this._request = (string path) => (FtpWebRequest)WebRequest.Create(FtpClientConfiguration.BuildPath(uri, path));
			this.AssertPath();
		}

		internal FtpClientConfiguration(Uri uri)
		{
			//this._request = (string path) => {
			//	UriBuilder uriBuilder = new UriBuilder(uri)
			//	{
			//		Path = FtpClientConfiguration.BuildPath(uriBuilder.Path, path)
			//	};
			//	return (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
			//};
			this.AssertPath();
		}

		private void AssertPath()
		{
			this._request(string.Empty);
		}

		private static string BuildPath(string basePath, string appendPath)
		{
			return string.Concat(basePath, appendPath);
		}

		internal FtpWebRequest CreateRequest(string path)
		{
			FtpWebRequest ftpWebRequest = this._request(path);
			if (this._credentials != null)
			{
				ftpWebRequest.Credentials = this._credentials;
			}
			return ftpWebRequest;
		}

		public FtpClientConfiguration Credentials(string username, string password)
		{
			this._credentials = new NetworkCredential(username, password);
			return this;
		}
	}
}