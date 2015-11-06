using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Casper
{
	public abstract class ProjectBase
	{		
		private readonly TaskCollection tasks;
		private readonly ProjectCollection subprojects;
		protected readonly ProjectBase parent;
		private readonly DirectoryInfo location;

		protected ProjectBase(ProjectBase parent, DirectoryInfo location) : this(parent, location, location.Name) {
		}

		protected ProjectBase(ProjectBase parent, DirectoryInfo location, string name) {
			this.parent = parent;
			this.location = location;
			this.Name = name;
			this.PathPrefix = null == parent ? "" : parent.PathPrefix + this.Name + ":";
			this.PathDescription = null == parent ? "root project" : "project '" + parent.PathPrefix + this.Name + "'";
			this.subprojects = new ProjectCollection(this);
			this.tasks = new TaskCollection(this);
			if (null != parent) {
				parent.subprojects.Add(this);
			}
		}

		public abstract void Configure();

		public void AddTask(string name, TaskBase task) {
			task.Name = name;
			task.Project = this;
			tasks.Add(task);
		}

		public string Name {
			get;
			private set;
		}

		private string PathPrefix {
			get;
			set;
		}

		public string PathDescription {
			get;
			private set;
		}

		public TaskCollection Tasks {
			get { return tasks; }
		}

		public void Execute(TaskBase task) {
			var currentDirectory = Directory.GetCurrentDirectory();
			try {
				Directory.SetCurrentDirectory(location.FullName);
				task.Execute();
			} finally {
				Directory.SetCurrentDirectory(currentDirectory);
			}
		}

		public void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
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
			string[] path = name.Split(':');
			return GetTaskByPath(new Queue<string>(path));
		}

		private TaskBase GetTaskByPath(Queue<string> path) {
			if (path.Count > 1) {
				var projectName = path.Dequeue();
				return this.subprojects[projectName].GetTaskByPath(path);
			} else {
				return this.tasks[path.Dequeue()];
			}
		}
	}
}
