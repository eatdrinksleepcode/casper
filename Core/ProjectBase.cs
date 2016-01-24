using System;
using System.Collections.Generic;
using System.Linq;
using Casper.IO;

namespace Casper
{
	public abstract class ProjectBase
	{		
		private readonly TaskCollection tasks;
		private readonly ProjectCollection subprojects;
		protected readonly ProjectBase parent;
		private readonly IDirectory location;
		private readonly IFileSystem fileSystem;

		protected ProjectBase(ProjectBase parent, string location, IFileSystem fileSystem) : this(parent, location, fileSystem, System.IO.Path.GetFileName(location)) {
		}

		protected ProjectBase(ProjectBase parent, string location, IFileSystem fileSystem, string name) {
			this.parent = parent;
			this.location = fileSystem.Directory(location);
			this.Name = name;
			this.PathPrefix = null == parent ? "" : parent.PathPrefix + this.Name + ":";
			this.PathDescription = null == parent ? "root project" : "project '" + parent.PathPrefix + this.Name + "'";
			this.subprojects = new ProjectCollection(this);
			this.tasks = new TaskCollection(this);
			this.fileSystem = fileSystem;
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

		public IFile File(string path) {
			return fileSystem.File(path);
		}

		public string Name {
			get;
			private set;
		}

		public string PathPrefix {
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
			ExecuteInProjectDirectory(task);
		}

		public void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
			var tasksToExecute = taskNamesToExecute.Select(a => this.GetTaskByName(a)).ToArray();
			var taskGraphClosure = tasksToExecute.SelectMany(t => t.AllDependencies()).Distinct().ToArray();
			Array.Sort(taskGraphClosure, (t1, t2) => t1.AllDependencies().Contains(t2) ? 1 : t2.AllDependencies().Contains(t1) ? -1 : 0);
			foreach (var task in taskGraphClosure) {
				Console.WriteLine(task.Path);
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

		private void ExecuteInProjectDirectory(TaskBase task) {
			var currentDirectory = fileSystem.GetCurrentDirectory();
			try {
				location.SetAsCurrent();
				task.Execute(fileSystem);
			}
			finally {
				currentDirectory.SetAsCurrent();
			}
		}
	}
}
