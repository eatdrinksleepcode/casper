using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Casper {
	public class TaskExecutionGraph {

		private readonly IEnumerable<TaskBase> tasksInOrder;

		public TaskExecutionGraph(params TaskBase[] tasksInOrder)
			: this((IEnumerable<TaskBase>)tasksInOrder) {
		}

		public TaskExecutionGraph(IEnumerable<TaskBase> tasksInOrder) {
			this.tasksInOrder = tasksInOrder;
		}

		public void ExecuteTasks() {
			ExecuteTasksInOrder();
		}

		private class TaskTextWriter : TextWriter {
			readonly Func<bool> getHasWritten;
			readonly Action setHasWritten;
			readonly TextWriter target;

			public TaskTextWriter(TextWriter target, Func<bool> getHasWritten, Action setHasWritten) : base() {
				this.target = target;
				this.getHasWritten = getHasWritten;
				this.setHasWritten = setHasWritten;
			}

			public override void Write(char value) {
				if(!getHasWritten()) {
					target.WriteLine();
					setHasWritten();
				}
				target.Write(value);
			}

			public override Encoding Encoding {
				get {
					return target.Encoding;
				}
			}
		}

		void ExecuteTasksInOrder() {
			foreach(var task in tasksInOrder) {
				Console.Write(task.Path);
				ExecuteTaskInProject(task);
			}
		}

		private class RedirectedStreams : IDisposable {
			private readonly IDisposable standardOut;
			private readonly IDisposable standardError;
			private bool hasWritten;

			public RedirectedStreams() {
				Func<TextWriter, TextWriter> redirectTarget = target => new TaskTextWriter(target, () => hasWritten, () => { hasWritten = true; });
				standardOut = RedirectedStandardOutput.RedirectOut(redirectTarget);
				standardError = RedirectedStandardOutput.RedirectError(redirectTarget);
			}

			public void Dispose() {
				if(null != standardOut) {
					standardOut.Dispose();
				}
				if(null != standardError) {
					standardError.Dispose();
				}
			}
		}

		void ExecuteTaskInProject(TaskBase task) {
			try {
				bool didWork;
				using(new RedirectedStreams()) {
					// HACK: this is awkward
					didWork = task.Project.Execute(task);
				}
				if(!didWork) {
					Console.Write(" (UP-TO-DATE)");
				}
			} finally {
				Console.WriteLine();
			}
		}
	}
}
