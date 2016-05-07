using System;
using System.Collections.Generic;

namespace Casper {
	public class TaskExecutionGraph {

		private readonly IEnumerable<TaskBase> tasksInOrder;

		public TaskExecutionGraph(IEnumerable<TaskBase> tasksInOrder) {
			this.tasksInOrder = tasksInOrder;
		}

		public void ExecuteTasks() {
			foreach(var task in tasksInOrder) {
				Console.WriteLine(task.Path);
				// HACK: this is awkward
				task.Project.Execute(task);
			}
		}
	}
}
