using System;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Extensions
{
	internal static class ObjectExtensions
	{
		public static void DisposeIfDisposable(this object instance)
		{
			IDisposable disposable = instance as IDisposable;
			if (disposable == null)
			{
				return;
			}
			disposable.Dispose();
		}
	}
}