using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Templating;

namespace Vertica.Integration.Domain.Monitoring
{
	internal class MonitorEmailTemplate : EmailTemplate
	{
		private readonly string _subject;

		private readonly MonitorEntry[] _entries;

		private readonly MonitorTarget _target;

		protected internal override bool IsHtml
		{
			get
			{
				return true;
			}
		}

		protected internal override System.Net.Mail.MailPriority? MailPriority
		{
			get
			{
				return this._target.MailPriority;
			}
		}

		protected internal override string Subject
		{
			get
			{
				return this._subject;
			}
		}

		public MonitorEmailTemplate(string subject, MonitorEntry[] entries, MonitorTarget target)
		{
			if (entries == null)
			{
				throw new ArgumentNullException("entries");
			}
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			this._subject = subject;
			this._entries = entries;
			this._target = target;
		}

		protected internal override string GetBody()
		{
			string str;
			using (MemoryStream memoryStream = new MemoryStream(Resources.EmailTemplate))
			{
				using (StreamReader streamReader = new StreamReader(memoryStream))
				{
					str = InMemoryRazorEngine.Execute<MonitorEntry[]>(streamReader.ReadToEnd(), this._entries, null, new Assembly[] { typeof(MonitorEmailTemplate).Assembly, typeof(HttpServerUtility).Assembly });
				}
			}
			return str;
		}
	}
}