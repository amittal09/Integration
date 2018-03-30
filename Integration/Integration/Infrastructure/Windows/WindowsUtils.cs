using System;
using System.Security.Principal;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal static class WindowsUtils
	{
		public static string GetIdentityName()
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			if (current == null)
			{
				return null;
			}
			return current.Name;
		}
	}
}