using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Model
{
    public class TaskRunner : ITaskRunner
    {
        private readonly ILogger _logger;

        private readonly TextWriter _outputter;

        public TaskRunner(ILogger logger, TextWriter outputter)
        {
            this._logger = logger;
            this._outputter = outputter;
        }

        public TaskExecutionResult Execute(ITask task, Arguments arguments = null)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            return (TaskExecutionResult)this.ExecuteInternal(task, arguments ?? Arguments.Empty);
        }

        private TaskExecutionResult ExecuteInternal<TWorkItem>(ITask<TWorkItem> task, Arguments arguments)
        {
            TWorkItem tWorkItem;
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            List<string> strs = new List<string>();
            Action<string> action = (string message) => {
                message = string.Format("[{0:HH:mm:ss}] {1}", Time.Now, message);
                this._outputter.WriteLine(message);
                strs.Add(message);
            };
            ILogger logger = this._logger;
            using (TaskLog taskLog = new TaskLog(task, new Action<LogEntry>(logger.LogEntry), new Output(action)))
            {
                try
                {
                    tWorkItem = task.Start(new TaskExecutionContext(new Log(new Action<string>(taskLog.LogMessage), this._logger), arguments));
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    taskLog.ErrorLog = this._logger.LogError(exception, null);
                    throw new TaskExecutionFailedException("Starting task failed.", exception);
                }
                try
                {
                    foreach (IStep<TWorkItem> step in task.Steps)
                    {
                        Execution execution = step.ContinueWith(tWorkItem);
                        if (execution != Execution.StepOut)
                        {
                            if (execution == Execution.StepOver)
                            {
                                continue;
                            }
                            using (StepLog stepLog = taskLog.LogStep(step))
                            {
                                try
                                {
                                    step.Execute(tWorkItem, new TaskExecutionContext(new Log(new Action<string>(stepLog.LogMessage), this._logger), arguments));
                                }
                                catch (Exception exception3)
                                {
                                    Exception exception2 = exception3;
                                    ErrorLog errorLog = this._logger.LogError(exception2, null);
                                    taskLog.ErrorLog = errorLog;
                                    stepLog.ErrorLog = errorLog;
                                    throw new TaskExecutionFailedException(string.Format("Step '{0}' failed.", stepLog.Name), exception2);
                                }
                            }
                        }
                        else
                        {
                            goto Label0;
                        }
                    }
                    Label0:
                    try
                    {
                        task.End(tWorkItem, new TaskExecutionContext(new Log(new Action<string>(taskLog.LogMessage), this._logger), arguments));
                    }
                    catch (Exception exception5)
                    {
                        Exception exception4 = exception5;
                        taskLog.ErrorLog = this._logger.LogError(exception4, null);
                        throw new TaskExecutionFailedException("Ending task failed.", exception4);
                    }
                }
                finally
                {
                    tWorkItem.DisposeIfDisposable();
                }
            }
            return new TaskExecutionResult(strs.ToArray());
        }
    }
}