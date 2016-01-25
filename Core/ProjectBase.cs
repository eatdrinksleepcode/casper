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
		private readonly Dictionary<string, TaskRecord> records;
		private readonly IFile taskRecordCache;

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
			this.taskRecordCache = fileSystem.Directory(location).Directory(".casper").File("tasks");
			this.records = taskRecordCache.Exists()
						? taskRecordCache.ReadAll<Dictionary<string, TaskRecord>>()
						: new Dictionary<string, TaskRecord>();
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

		[Serializable]
		private class TaskRecord {
			private TaskState input = new TaskState();
			private TaskState output = new TaskState();

			public void RecordInputs(TaskBase task) {
				input.RecordFiles(task.InputFiles);
			}

			public void RecordOutputs(TaskBase task) {
				output.RecordFiles(task.OutputFiles);
			}

			public bool IsUpToDate(TaskBase task) {
				var hasOutputs = task.OutputFiles.Count() > 0;
				var inputsUpToDate = input.UpToDate(task.InputFiles);
				var outputsUpToDate = output.UpToDate(task.OutputFiles);
				return hasOutputs && inputsUpToDate && outputsUpToDate;
			}
		}

		[Serializable]
		private class TaskState {
			private Dictionary<string, DateTimeOffset> fileStamps = new Dictionary<string, DateTimeOffset>();

			public void RecordFiles(IEnumerable<IFile> files) {
				foreach(var file in files) {
					fileStamps[file.Path] = file.LastWriteTimeUtc;
				}
			}

			public bool UpToDate(IEnumerable<IFile> files) {
				return files.All(UpToDate);
			}

			private bool UpToDate(IFile file) {
				DateTimeOffset modified;
				return fileStamps.TryGetValue(file.Path, out modified) && modified.CompareTo(file.LastWriteTimeUtc) == 0;
			}
		}

		public bool Execute(TaskBase task) {
			if(IsUpToDate(task)) {
				return false;
			}
			TaskRecord record = new TaskRecord();
			record.RecordInputs(task);
			ExecuteInProjectDirectory(task);
			record.RecordOutputs(task);
			SaveTaskState(task, record);
			return true;
		}

		private bool IsUpToDate(TaskBase task) {
			TaskRecord oldRecord;
			return records.TryGetValue(task.Name, out oldRecord) && oldRecord.IsUpToDate(task);
		}

		private void SaveTaskState(TaskBase task, TaskRecord record) {
			records[task.Name] = record;
			taskRecordCache.CreateDirectories();
			taskRecordCache.WriteAll(records);
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
