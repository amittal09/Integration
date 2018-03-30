using System;
using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITask
	{
		string Description
		{
			get;
		}

		IEnumerable<IStep> Steps
		{
			get;
		}
	}
}