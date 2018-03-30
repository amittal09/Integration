using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
	public class PingUrlsStep : Step<MonitorWorkItem>
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public override string Description
		{
			get
			{
				return "Pings a number of configurable urls by issuing a simple GET-request to every configured address expecting a http status code OK/200.";
			}
		}

		public PingUrlsStep(IHttpClientFactory httpClientFactory)
		{
			this._httpClientFactory = httpClientFactory;
		}

		private void AddException(PingUrlsStep.PingException exception, MonitorWorkItem workItem)
		{
			string str = string.Format("{0}\r\n\r\n{1}", exception.Message, exception.InnerException.AggregateMessages());
			workItem.Add(Time.UtcNow, this.Name(), str, new Target[0]);
		}

		private static PingUrlsStep.PingException[] AssertExceptions(AggregateException ex)
		{
			PingUrlsStep.PingException[] array = ex.Flatten().InnerExceptions.OfType<PingUrlsStep.PingException>().ToArray<PingUrlsStep.PingException>();
			if ((int)array.Length != ex.InnerExceptions.Count)
			{
				throw ex;
			}
			return array;
		}

		public override Execution ContinueWith(MonitorWorkItem workItem)
		{
			if (!workItem.Configuration.PingUrls.ShouldExecute)
			{
				return Execution.StepOver;
			}
			return Execution.Execute;
		}

		public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
		{
			Uri[] uriArray = PingUrlsStep.ParseUrls(workItem.Configuration.PingUrls.Urls, context.Log);
			if (uriArray.Length == 0)
			{
				return;
			}
			try
			{
				Parallel.ForEach<Uri>(uriArray, new ParallelOptions()
				{
					MaxDegreeOfParallelism = 4
				}, (Uri url) => {
					Stopwatch stopwatch = Stopwatch.StartNew();
					try
					{
						System.Threading.Tasks.Task<HttpResponseMessage> task = this.HttpGet(url, workItem.Configuration.PingUrls.MaximumWaitTimeSeconds);
						task.Wait();
						task.Result.EnsureSuccessStatusCode();
					}
					catch (Exception exception)
					{
						throw new PingUrlsStep.PingException(url, stopwatch, exception);
					}
				});
			}
			catch (AggregateException aggregateException)
			{
				PingUrlsStep.PingException[] pingExceptionArray = PingUrlsStep.AssertExceptions(aggregateException);
				for (int i = 0; i < (int)pingExceptionArray.Length; i++)
				{
					this.AddException(pingExceptionArray[i], workItem);
				}
			}
		}

        private async System.Threading.Tasks.Task<HttpResponseMessage> HttpGet(Uri absoluteUri, uint maximumWaitTimeSeconds)
        {
            HttpResponseMessage async;
            using (HttpClient httpClient = this._httpClientFactory.Create())
            {
                httpClient.Timeout = TimeSpan.FromSeconds((double)maximumWaitTimeSeconds);
                async = await httpClient.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);
            }
            return async;
        }

        private static Uri[] ParseUrls(string[] urls, ILog log)
		{
			Uri uri;
			List<Uri> uris = new List<Uri>();
			string[] strArrays = urls;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (!Uri.TryCreate(str, UriKind.Absolute, out uri))
				{
					log.Warning(Target.Service, "Skipping ping of \"{0}\". Could not parse value as an absolute URL.", new object[] { str });
				}
				else if (!uris.Contains(uri))
				{
					uris.Add(uri);
				}
			}
			return uris.ToArray();
		}

		private class PingException : Exception
		{
			public PingException(Uri uri, Stopwatch watch, Exception inner) : base(string.Format("Ping failed for URL {0} (running for {1} seconds).", uri, Math.Round(watch.Elapsed.TotalSeconds, 3)), inner)
			{
			}
		}
	}
}