using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Extensions.AttributeExt;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _repository;

        private readonly IArchiveService _archive;

        private readonly ILogger _logger;

        private readonly JsonSerializerSettings _serializerSettings;

        public ConfigurationService(IArchiveService archive, IConfigurationRepository repository, ILogger logger)
        {
            this._repository = repository;
            this._logger = logger;
            this._archive = archive;
            JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
            jsonSerializerSetting.TypeNameHandling = TypeNameHandling.Auto;
            this._serializerSettings = jsonSerializerSetting;
        }

        public ArchiveCreated Backup(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or empty.", "id");
            }
            Vertica.Integration.Infrastructure.Configuration.Configuration configuration = this._repository.Get(id);
            if (configuration == null)
            {
                return null;
            }
            return this._archive.Archive(configuration.Name, (BeginArchive archive) =>
            {
                archive.Options.GroupedBy("Backup").ExpiresAfterMonths(1);
                archive.IncludeContent("data", configuration.JsonData, ".json");
                string newLine = Environment.NewLine;
                string[] str = new string[] { configuration.Id, configuration.Name, configuration.Description, null, null };
                str[3] = configuration.Updated.ToString();
                str[4] = configuration.UpdatedBy;
                archive.IncludeContent("meta", string.Join(newLine, str), null);
            });
        }

        public TConfiguration Get<TConfiguration>()
        where TConfiguration : class, new()
        {
            if (typeof(TConfiguration) == typeof(Vertica.Integration.Infrastructure.Configuration.Configuration))
            {
                throw new ArgumentException("Getting a Configuration of type Configuration is not allowed.");
            }
            Vertica.Integration.Infrastructure.Configuration.Configuration configuration = this._repository.Get(this.GetId<TConfiguration>(false));
            if (configuration != null)
            {
                return JsonConvert.DeserializeObject<TConfiguration>(configuration.JsonData, this._serializerSettings);
            }
            TConfiguration tConfiguration = Activator.CreateInstance<TConfiguration>();
            this.Save<TConfiguration>(tConfiguration, "IntegrationService", false);
            return tConfiguration;
        }

        private string GetDescription(Type configurationType)
        {
            if (configurationType == null)
            {
                return null;
            }
            DescriptionAttribute attribute = configurationType.GetAttribute<DescriptionAttribute>(false);
            if (attribute == null)
            {
                return null;
            }
            return attribute.Description.NullIfEmpty();
        }

        internal static string GetGuidId<TConfiguration>()
        {
            Guid guid;
            GuidAttribute attribute = typeof(TConfiguration).GetAttribute<GuidAttribute>(false);
            if (attribute == null || !Guid.TryParse(attribute.Value, out guid))
            {
                return null;
            }
            return guid.ToString("D");
        }

        private string GetId<TConfiguration>(bool warnIfMissingGuid = false)
        {
            string guidId = ConfigurationService.GetGuidId<TConfiguration>();
            if (guidId != null)
            {
                return guidId;
            }
            Type type = typeof(TConfiguration);
            guidId = string.Join(", ", new string[] { type.FullName, type.Assembly.GetName().Name });
            if (warnIfMissingGuid)
            {
                this._logger.LogWarning(Target.Service, "Class '{0}' used for configuration should have been decorated with a [Guid(\"[insert-new-Guid-here]\")]-attribute.\r\nThis is to ensure a unique and Refactor-safe Global ID.\r\n\r\nRemember when (or if) you add this Guid-attribute, that you (manually) have to merge the data to the new instance.\r\nIf you don't like to do it manually, you can of course use a Migration.\r\n\r\nIMPORTANT: Remember to use the \"D\" format for Guids, e.g. 1EB3F675-C634-412F-A76F-FC3F9A4A68D5", new object[] { guidId });
            }
            return guidId;
        }

        public TConfiguration Save<TConfiguration>(TConfiguration configuration, string updatedBy, bool createArchiveBackup = false)
        where TConfiguration : class, new()
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (string.IsNullOrWhiteSpace(updatedBy))
            {
                throw new ArgumentException("Value cannot be null or empty.", "updatedBy");
            }
            if ((object)configuration is Vertica.Integration.Infrastructure.Configuration.Configuration)
            {
                throw new ArgumentException("Use the specific Save method when saving this Configuration instance.", "configuration");
            }
            string id = this.GetId<TConfiguration>(true);
            if (createArchiveBackup)
            {
                this.Backup(id);
            }
            Type type = typeof(TConfiguration);
            this._repository.Save(new Vertica.Integration.Infrastructure.Configuration.Configuration()
            {
                Id = id,
                Name = type.Name,
                Description = this.GetDescription(type),
                JsonData = JsonConvert.SerializeObject(configuration, Formatting.Indented, this._serializerSettings),
                UpdatedBy = updatedBy
            });
            return this.Get<TConfiguration>();
        }
    }
}