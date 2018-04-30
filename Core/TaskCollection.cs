using System.Collections.Generic;

namespace Casper {
	public class TaskCollection : IEnumerable<TaskBase> {

		private readonly ProjectBase project;
		private readonly Dictionary<string, TaskBase> tasks = new Dictionary<string, TaskBase>();

		public TaskCollection(ProjectBase project) {
			this.project = project;
		}

		internal void Add(TaskBase task) {
			tasks.Add(task.Name, task);
		}

		public TaskBase this[string name] {
			get {
				if (!TryGetValue(name, out var result)) {
					throw new UnknownTaskException(project, name);
				}
				return result;
			}
		}

		public int Count => tasks.Count;

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
