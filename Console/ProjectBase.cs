using System.Collections.Generic;
using System.Linq;


namespace Casper
{
	public abstract class ProjectBase
	{
		private readonly Dictionary<string, TaskBase> tasks = new Dictionary<string, TaskBase>();
		private readonly List<ProjectBase> subprojects = new List<ProjectBase>();
		protected readonly ProjectBase parent;

		protected ProjectBase(ProjectBase parent) {
			if (null != parent) {
				parent.subprojects.Add(this);
			}
			this.parent = parent;
		}

		public abstract void Configure();

		public void AddTask(string name, TaskBase task) {
			tasks.Add(name, task);
		}

		public IEnumerable<KeyValuePair<string, TaskBase>> GetTasks() {
			return tasks;
		}

		public TaskBase GetTaskByName(string name) {
			TaskBase result = GetTaskByNameIncludingSubProjects(name);
			if (null == result) {
				throw new CasperException(CasperException.EXIT_CODE_MISSING_TASK, "Task '{0}' does not exist", name);
			}
			return result;
		}

		private TaskBase GetTaskByNameIncludingSubProjects(string name) {
			TaskBase result;
			if (!tasks.TryGetValue(name, out result)) {
				result = subprojects.Select(p => p.GetTaskByNameIncludingSubProjects(name)).FirstOrDefault(t => null != t);
			}
			return result;
		}
	}
}
