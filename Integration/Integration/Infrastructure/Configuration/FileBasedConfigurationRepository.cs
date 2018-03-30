using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Configuration
{
	internal class FileBasedConfigurationRepository : IConfigurationRepository
	{
		private readonly string _baseDirectory;

		private readonly ILogger _logger;

		public const string BaseDirectoryKey = "FileBasedConfigurationRepository.BaseDirectory";

		public FileBasedConfigurationRepository(IRuntimeSettings settings, ILogger logger)
		{
			this._baseDirectory = settings["FileBasedConfigurationRepository.BaseDirectory"].NullIfEmpty() ?? "Data\\Configurations";
			if (!Directory.Exists(this._baseDirectory))
			{
				Directory.CreateDirectory(this._baseDirectory);
			}
			this._logger = logger;
		}

		private FileInfo ConfigurationFilePath(string id)
		{
			return new FileInfo(Path.Combine(this._baseDirectory, string.Format("{0}.json", id)));
		}

		public void Delete(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("Value cannot be null or empty.", "id");
			}
			FileInfo fileInfo = this.ConfigurationFilePath(id);
			if (fileInfo.Exists)
			{
				FileInfo fileInfo1 = FileBasedConfigurationRepository.MetaFilePath(fileInfo);
				fileInfo.Delete();
				if (fileInfo1.Exists)
				{
					fileInfo1.Delete();
				}
			}
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration Get(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("Value cannot be null or empty.", "id");
			}
			FileInfo fileInfo = this.ConfigurationFilePath(id);
			if (!fileInfo.Exists)
			{
				return null;
			}
			return this.Map(fileInfo);
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration[] GetAll()
		{
			return (new DirectoryInfo(this._baseDirectory)).EnumerateFiles("*.json").Select<FileInfo, Vertica.Integration.Infrastructure.Configuration.Configuration>(new Func<FileInfo, Vertica.Integration.Infrastructure.Configuration.Configuration>(this.Map)).ToArray<Vertica.Integration.Infrastructure.Configuration.Configuration>();
		}

		private Vertica.Integration.Infrastructure.Configuration.Configuration Map(FileInfo configurationFile)
		{
			string description;
			string updatedBy;
			FileBasedConfigurationRepository.MetaFile metaFile = this.ReadMetaFile(configurationFile);
			Vertica.Integration.Infrastructure.Configuration.Configuration configuration = new Vertica.Integration.Infrastructure.Configuration.Configuration()
			{
				Id = Path.GetFileNameWithoutExtension(configurationFile.Name),
				JsonData = File.ReadAllText(configurationFile.FullName),
				Created = configurationFile.CreationTimeUtc,
				Updated = configurationFile.LastWriteTimeUtc,
				Name = (metaFile != null ? metaFile.Name : configurationFile.Name)
			};
			if (metaFile != null)
			{
				description = metaFile.Description;
			}
			else
			{
				description = null;
			}
			configuration.Description = description;
			if (metaFile != null)
			{
				updatedBy = metaFile.UpdatedBy;
			}
			else
			{
				updatedBy = null;
			}
			configuration.UpdatedBy = updatedBy;
			return configuration;
		}

		private static FileInfo MetaFilePath(FileInfo archiveFile)
		{
			return new FileInfo(Path.Combine(new string[] { string.Format("{0}.meta", archiveFile.FullName) }));
		}

		private FileBasedConfigurationRepository.MetaFile ReadMetaFile(FileInfo archiveFile)
		{
			FileInfo fileInfo = FileBasedConfigurationRepository.MetaFilePath(archiveFile);
			if (!fileInfo.Exists)
			{
				return null;
			}
			string str = File.ReadAllText(fileInfo.FullName);
			if (string.IsNullOrWhiteSpace(str))
			{
				return null;
			}
			return FileBasedConfigurationRepository.MetaFile.FromJson(str, this._logger);
		}

		public Vertica.Integration.Infrastructure.Configuration.Configuration Save(Vertica.Integration.Infrastructure.Configuration.Configuration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			FileInfo fileInfo = this.ConfigurationFilePath(configuration.Id);
			File.WriteAllText(fileInfo.FullName, configuration.JsonData);
			File.WriteAllText(FileBasedConfigurationRepository.MetaFilePath(fileInfo).FullName, (new FileBasedConfigurationRepository.MetaFile(configuration)).ToString());
			return this.Get(configuration.Id);
		}

		private class MetaFile
		{
			public string Description
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public string UpdatedBy
			{
				get;
				set;
			}

			public MetaFile(Vertica.Integration.Infrastructure.Configuration.Configuration configuration = null)
			{
				if (configuration != null)
				{
					this.Name = configuration.Name;
					this.Description = configuration.Description;
					this.UpdatedBy = configuration.UpdatedBy;
				}
			}

			public static FileBasedConfigurationRepository.MetaFile FromJson(string json, ILogger logger)
			{
				FileBasedConfigurationRepository.MetaFile metaFile;
				if (string.IsNullOrWhiteSpace(json))
				{
					throw new ArgumentException("Value cannot be null or empty.", "json");
				}
				if (logger == null)
				{
					throw new ArgumentNullException("logger");
				}
				try
				{
					metaFile = JsonConvert.DeserializeObject<FileBasedConfigurationRepository.MetaFile>(json);
				}
				catch (Exception exception)
				{
					logger.LogError(exception, null);
					metaFile = null;
				}
				return metaFile;
			}

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this, Formatting.Indented);
			}
		}
	}
}