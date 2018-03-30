using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
	[Serializable]
	public class TaskNotFoundException : Exception
	{
		public TaskNotFoundException()
		{
		}

		internal TaskNotFoundException(string taskName) : base(string.Format("Task with name '{0}' not found. \r\nConsider running the \"WriteDocumentationTask\" to get a text-output of available tasks.\r\nIf your task is not listed then you probably forgot to register it. Use Tasks(tasks => tasks.Task<YourTask>)", taskName))
		{
		}

		protected TaskNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}