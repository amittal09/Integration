using System;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public interface IConfigurationRepository
	{
		void Delete(string id);

		Vertica.Integration.Infrastructure.Configuration.Configuration Get(string id);

		Vertica.Integration.Infrastructure.Configuration.Configuration[] GetAll();

		Vertica.Integration.Infrastructure.Configuration.Configuration Save(Vertica.Integration.Infrastructure.Configuration.Configuration configuration);
	}
}