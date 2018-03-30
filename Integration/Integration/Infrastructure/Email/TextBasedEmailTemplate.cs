using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Vertica.Integration.Infrastructure.Email
{
	public class TextBasedEmailTemplate : EmailTemplate
	{
		private readonly string _subject;

		private readonly StringBuilder _text;

		private readonly List<Attachment> _attachments;

		private System.Net.Mail.MailPriority? _priority;

		protected internal override IEnumerable<Attachment> Attachments
		{
			get
			{
				return this._attachments;
			}
		}

		protected internal override bool IsHtml
		{
			get
			{
				return false;
			}
		}

		protected internal override System.Net.Mail.MailPriority? MailPriority
		{
			get
			{
				return this._priority;
			}
		}

		protected internal override string Subject
		{
			get
			{
				return this._subject;
			}
		}

		public TextBasedEmailTemplate(string subject, params object[] args)
		{
			this._subject = string.Format(subject, args);
			this._text = new StringBuilder();
			this._attachments = new List<Attachment>();
		}

		public TextBasedEmailTemplate AddAttachment(Attachment attachment)
		{
			if (attachment == null)
			{
				throw new ArgumentNullException("attachment");
			}
			this._attachments.Add(attachment);
			return this;
		}

		protected internal override string GetBody()
		{
			return this._text.ToString();
		}

		public TextBasedEmailTemplate Priority(System.Net.Mail.MailPriority priority)
		{
			this._priority = new System.Net.Mail.MailPriority?(priority);
			return this;
		}

		public TextBasedEmailTemplate Write(string format, params object[] args)
		{
			this._text.AppendFormat(format, args);
			return this;
		}

		public TextBasedEmailTemplate WriteLine(string format, params object[] args)
		{
			this.Write(format, args);
			this._text.AppendLine();
			return this;
		}
	}
}