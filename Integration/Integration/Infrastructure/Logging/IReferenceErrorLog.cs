namespace Vertica.Integration.Infrastructure.Logging
{
	public interface IReferenceErrorLog
	{
		Vertica.Integration.Infrastructure.Logging.ErrorLog ErrorLog
		{
			get;
		}
	}
}