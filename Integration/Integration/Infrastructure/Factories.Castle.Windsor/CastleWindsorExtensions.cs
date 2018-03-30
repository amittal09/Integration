using Castle.MicroKernel.Registration;
using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor
{
	public static class CastleWindsorExtensions
	{
		public static BasedOnDescriptor Expose(this BasedOnDescriptor descriptor, Action<Type> expose)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException("descriptor");
			}
			if (expose == null)
			{
				throw new ArgumentNullException("expose");
			}
			return descriptor.If((Type x) => {
				expose(x);
				return true;
			});
		}
	}
}