using System.Collections.Generic;

namespace Casper {
	public class TaskCollection : IEnumerable<TaskBase> {

		private readonly Dictionary<string, TaskBase> tasks = new Dictionary<string, TaskBase>();

		internal void Add(TaskBase task) {
			tasks.Add(task.Name, task);
		}

		public TaskBase this[string name] {
			get { return tasks[name]; }
		}

		public bool TryGetValue(string name, out TaskBase task) {
			return tasks.TryGetValue(name, out task);
		}

		public IEnumerator<TaskBase> GetEnumerator() {
			return tasks.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
