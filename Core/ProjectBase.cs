using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Casper
{
	public abstract class ProjectBase
	{		
		private readonly TaskCollection tasks = new TaskCollection();
		private readonly List<ProjectBase> subprojects = new List<ProjectBase>();
		protected readonly ProjectBase parent;
		private readonly string location;

		protected ProjectBase(ProjectBase parent, string location) {
			if (null != parent) {
				parent.subprojects.Add(this);
			}
			this.parent = parent;
			this.location = location;
		}

		public abstract void Configure();

		public void AddTask(string name, TaskBase task) {
			task.Name = name;
			task.Project = this;
			tasks.Add(task);
		}

		public TaskCollection Tasks {
			get { return tasks; }
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

		public void Execute(TaskBase task) {
			var currentDirectory = Directory.GetCurrentDirectory();
			try {
				Directory.SetCurrentDirectory(location);
				task.Execute();
			} finally {
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}
	}
}
