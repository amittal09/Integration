using System;
using System.Reflection;

namespace Vertica.Integration
{
	public interface IRuntimeSettings
	{
		ApplicationEnvironment Environment
		{
			get;
		}

		string this[string name]
		{
			get;
		}
	}
}