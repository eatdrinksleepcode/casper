using System;
using System.Collections.Generic;

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
			foreach(var task in tasksInOrder) {
				Console.Write(task.Path);
				try {
					// HACK: this is awkward
					if(!task.Project.Execute(task)) {
						Console.Write(" (UP-TO-DATE)");
					}
				} finally {
					Console.WriteLine();
				}
			}
		}
	}
}
