namespace Vertica.Integration.Model
{
	public interface ITaskExecutionContext
	{
		Vertica.Integration.Model.Arguments Arguments
		{
			get;
		}

		ILog Log
		{
			get;
		}
	}
}