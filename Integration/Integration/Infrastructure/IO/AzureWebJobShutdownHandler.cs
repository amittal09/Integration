using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class AzureWebJobShutdownHandler : FilesChangedHandler
	{
		private const string EnvironmentVariableName = "WEBJOBS_SHUTDOWN_FILE";

		public AzureWebJobShutdownHandler() : base(AzureWebJobShutdownHandler.GetDirectoryToWatch())
		{
		}

		private static DirectoryInfo GetDirectoryToWatch()
		{
			string str = AzureWebJobShutdownHandler.ShutdownFile();
			if (string.IsNullOrWhiteSpace(str))
			{
				throw new InvalidOperationException(string.Format("Environment variable {0} is null or empty.", "WEBJOBS_SHUTDOWN_FILE"));
			}
			return new DirectoryInfo(Path.GetDirectoryName(str) ?? string.Empty);
		}

		public static bool IsRunningOnAzure()
		{
			return !string.IsNullOrEmpty(AzureWebJobShutdownHandler.ShutdownFile());
		}

		private static string ShutdownFile()
		{
			return Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");
		}
	}
}