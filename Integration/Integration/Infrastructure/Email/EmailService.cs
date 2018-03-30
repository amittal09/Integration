using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mail;

namespace Vertica.Integration.Infrastructure.Email
{
	public class EmailService : IEmailService
	{
		public EmailService()
		{
		}

		public void Send(EmailTemplate template, params string[] recipients)
		{
			if (template == null)
			{
				throw new ArgumentNullException("template");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			using (MailMessage mailMessage = new MailMessage())
			{
				using (SmtpClient smtpClient = new SmtpClient())
				{
					mailMessage.Subject = template.Subject;
					mailMessage.Body = template.GetBody();
					mailMessage.IsBodyHtml = template.IsHtml;
					if (template.MailPriority.HasValue)
					{
						mailMessage.Priority = template.MailPriority.Value;
					}
					string[] strArrays = recipients;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						if (!string.IsNullOrWhiteSpace(str))
						{
							mailMessage.To.Add(str.Trim());
						}
					}
					object attachments = template.Attachments;
					if (attachments == null)
					{
						attachments = new Attachment[0];
					}
					foreach (Attachment attachment in (IEnumerable<Attachment>)attachments)
					{
						mailMessage.Attachments.Add(attachment);
					}
					smtpClient.Send(mailMessage);
				}
			}
		}
	}
}