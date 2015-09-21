using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public void Execute(TaskBase task) {
			var currentDirectory = Directory.GetCurrentDirectory();
			try {
				Directory.SetCurrentDirectory(location);
				task.Execute();
			} finally {
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}

		protected void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
			var tasks = taskNamesToExecute.Select(a => this.GetTaskByName(a)).ToArray();
			var taskGraphClosure = tasks.SelectMany(t => t.AllDependencies()).Distinct().ToArray();
			Array.Sort(taskGraphClosure, (t1, t2) => t1.AllDependencies().Contains(t2) ? 1 : t2.AllDependencies().Contains(t1) ? -1 : 0);
			foreach (var task in taskGraphClosure) {
				Console.WriteLine(task.Name + ":");
				// HACK: this is awkward
				task.Project.Execute(task);
			}
		}

		private TaskBase GetTaskByName(string name) {
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