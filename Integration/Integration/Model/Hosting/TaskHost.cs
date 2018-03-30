using System;
using System.IO;
using System.Runtime.CompilerServices;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Model.Hosting
{
    public class TaskHost : IHost
    {
        private readonly ITaskFactory _factory;

        private readonly ITaskRunner _runner;

        private readonly IWindowsServiceHandler _windowsService;

        private readonly IScheduledTaskHandler _scheduledTask;

        private readonly TextWriter _textWriter;

        public string Description
        {
            get
            {
                return "Handles execution of Tasks.";
            }
        }

        public TaskHost(ITaskFactory factory, ITaskRunner runner, IWindowsServiceHandler windowsService, IScheduledTaskHandler scheduledTask, TextWriter textWriter)
        {
            this._factory = factory;
            this._runner = runner;
            this._windowsService = windowsService;
            this._scheduledTask = scheduledTask;
            this._textWriter = textWriter;
        }

        public bool CanHandle(HostArguments args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            if (string.IsNullOrWhiteSpace(args.Command))
            {
                return false;
            }
            return this._factory.Exists(args.Command);
        }

        public void Handle(HostArguments args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            ITask task = this._factory.Get(args.Command);
            if (this.InstallOrRunAsWindowsService(args, task))
            {
                return;
            }
            if (this.InstallAsScheduledTask(args, task))
            {
                return;
            }
            this._runner.Execute(task, args.Args);
        }

        private bool InstallAsScheduledTask(HostArguments args, ITask task)
        {
            return this._scheduledTask.Handle(args, task);
        }

        private bool InstallOrRunAsWindowsService(HostArguments args, ITask task)
        {
            Action action2 = null;
            Func<IDisposable> func = () => {
                string str;
                TimeSpan timeSpan;
                args.CommandArgs.TryGetValue("interval", out str);
                if (!TimeSpan.TryParse(str, out timeSpan))
                {
                    timeSpan = TimeSpan.FromMinutes(1);
                }
                TimeSpan timeSpan1 = timeSpan;
                Action u003cu003e9_1 = action2;
                if (u003cu003e9_1 == null)
                {
                    Action action = () => this._runner.Execute(task, args.Args);
                    Action action1 = action;
                    action2 = action;
                    u003cu003e9_1 = action1;
                }
                return timeSpan1.Repeat(u003cu003e9_1, this._textWriter);
            };
            return this._windowsService.Handle(args, new HandleAsWindowsService(task.Name(), task.Name(), task.Description, func));
        }
    }
}