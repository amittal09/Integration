using System;
using System.Net.Http;

namespace Vertica.Integration.Infrastructure.Remote
{
	public class HttpClientFactory : IHttpClientFactory
	{
		public HttpClientFactory()
		{
		}

		public HttpClient Create()
		{
			return new HttpClient(new HttpClientHandler()
			{
				UseDefaultCredentials = true
			});
		}
	}
}