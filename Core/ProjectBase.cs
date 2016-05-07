using System;
using System.Collections.Generic;
using System.Linq;
using Casper.IO;

namespace Casper {
	public abstract class ProjectBase {
		private readonly TaskCollection tasks;
		private readonly ProjectCollection subprojects;
		protected readonly ProjectBase parent;
		private readonly IDirectory location;
		private readonly IFileSystem fileSystem;
		private readonly Dictionary<string, TaskRecord> records;
		private readonly IFile taskRecordCache;

		protected ProjectBase(IFileSystem fileSystem, string location)
			: this(fileSystem, location, System.IO.Path.GetFileName(location)) {
		}

		protected ProjectBase(IFileSystem fileSystem, string location, string name)
			: this(null, location, fileSystem, name) {
		}

		protected ProjectBase(ProjectBase parent, string location)
			: this(parent, location, System.IO.Path.GetFileName(location)) {
		}

		protected ProjectBase(ProjectBase parent, string location, string name)
			: this(parent, location, parent.fileSystem, name) {
		}

		private ProjectBase(ProjectBase parent, string location, IFileSystem fileSystem, string name) {
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
			if(null != parent) {
				parent.subprojects.Add(this);
			}
		}

		protected virtual void Configure() { }

		public void ConfigureAll() {
			Configure();
			foreach(var project in subprojects) {
				project.ConfigureAll();
			}
		}

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

		public ProjectCollection Projects {
			get { return subprojects; }
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
				return ToHashSet(fileStamps.Select(x => new { Path = x.Key, TimeStamp = x.Value })).SetEquals(files.Select(x => new { Path = x.Path, TimeStamp = x.LastWriteTimeUtc }));
			}

			private static HashSet<T> ToHashSet<T>(IEnumerable<T> source) {
				return new HashSet<T>(source);
			}
		}

		public TaskExecutionGraph BuildTaskExecutionGraph(IEnumerable<string> taskNamesToExecute) {
			IEnumerable<TaskBase> tasksToExecute = ResolveTaskNames(taskNamesToExecute);
			var taskGraph = GenerateTaskGraphTraversalOrder(tasksToExecute);
			return taskGraph;
		}

		private IEnumerable<TaskBase> ResolveTaskNames(IEnumerable<string> taskNamesToExecute) {
			return taskNamesToExecute.Select(a => this.GetTaskByName(a)).ToArray();
		}

		private TaskExecutionGraph GenerateTaskGraphTraversalOrder(IEnumerable<TaskBase> tasksToExecute) {
			var tasksInOrder = new List<TaskBase>();
			AddTasksToExecutionPlan(tasksToExecute, tasksInOrder);
			return new TaskExecutionGraph(tasksInOrder);
		}

		private void AddTasksToExecutionPlan(IEnumerable<TaskBase> tasksToExecute, List<TaskBase> tasksInOrder) {
			foreach(var task in tasksToExecute) {
				if(!tasksInOrder.Contains(task)) {
					AddTasksToExecutionPlan(task.DependsOn.Cast<TaskBase>(), tasksInOrder);
					tasksInOrder.Add(task);
				}
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

		private TaskBase GetTaskByName(string name) {
			string[] path = name.Split(':');
			return GetTaskByPath(new Queue<string>(path));
		}

		private TaskBase GetTaskByPath(Queue<string> path) {
			if(path.Count > 1) {
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
			} finally {
				currentDirectory.SetAsCurrent();
			}
		}
	}
}
