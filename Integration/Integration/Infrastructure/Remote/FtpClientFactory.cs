using System;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Infrastructure.Remote
{
	public class FtpClientFactory : IFtpClientFactory
	{
		public FtpClientFactory()
		{
		}

		public IFtpClient Create(Uri ftpUri, Action<FtpClientConfiguration> ftp = null)
		{
			FtpClientConfiguration ftpClientConfiguration = new FtpClientConfiguration(ftpUri);
			if (ftp != null)
			{
				ftp(ftpClientConfiguration);
			}
			return new FtpClient(ftpClientConfiguration);
		}

		public IFtpClient Create(string ftpUri, Action<FtpClientConfiguration> ftp = null)
		{
			FtpClientConfiguration ftpClientConfiguration = new FtpClientConfiguration(ftpUri);
			if (ftp != null)
			{
				ftp(ftpClientConfiguration);
			}
			return new FtpClient(ftpClientConfiguration);
		}
	}
}