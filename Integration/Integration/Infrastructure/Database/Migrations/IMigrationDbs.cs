using System;
using System.Collections;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public interface IMigrationDbs : IEnumerable<MigrationDb>, IEnumerable
	{
		bool CheckExistsAndCreateIntegrationDbIfNotFound
		{
			get;
		}

		DatabaseServer IntegrationDbDatabaseServer
		{
			get;
		}

		bool IntegrationDbDisabled
		{
			get;
		}

		IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb);
	}
}