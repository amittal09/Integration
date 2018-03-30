using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Email
{
	public abstract class EmailTemplate
	{
		protected internal virtual IEnumerable<Attachment> Attachments
		{
            get;
		}

		protected internal abstract bool IsHtml
		{
			get;
		}

		protected internal virtual System.Net.Mail.MailPriority? MailPriority
		{
			get
			{
				return null;
			}
		}

		protected internal abstract string Subject
		{
			get;
		}

		protected EmailTemplate()
		{
		}

		protected internal abstract string GetBody();
	}
}