using Castle.Windsor;
using System;
using System.Runtime.CompilerServices;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	public class EventLoggerConfiguration : IInitializable<IWindsorContainer>
	{
		internal string SourceName
		{
			get;
			private set;
		}

		internal EventLoggerConfiguration()
		{
		}

		public EventLoggerConfiguration Source(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or empty.", "name");
			}
			this.SourceName = name;
			return this;
		}

		void Vertica.Integration.IInitializable<Castle.Windsor.IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance<EventLoggerConfiguration>(this);
		}
	}
}