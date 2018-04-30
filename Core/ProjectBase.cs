using System;
using System.Collections.Generic;
using System.Linq;
using Casper.IO;
using System.Diagnostics;

namespace Casper {
	[DebuggerDisplay("{" + nameof(PathDescription) + "}")]
	public abstract class ProjectBase {
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
			this.PathPrefix = (null == parent ? "" : parent.PathPrefix + this.Name) + ":";
			this.PathDescription = null == parent ? "root project" : "project '" + parent.PathPrefix + this.Name + "'";
			this.Projects = new ProjectCollection(this);
			this.Tasks = new TaskCollection(this);
			this.fileSystem = fileSystem;
			this.taskRecordCache = fileSystem.Directory(location).Directory(".casper").File("tasks");
			this.records = taskRecordCache.Exists()
						? taskRecordCache.ReadAll<Dictionary<string, TaskRecord>>()
						: new Dictionary<string, TaskRecord>();
			parent?.Projects.Add(this);
		}

		protected virtual void Configure() { }

		public void ConfigureAll() {
			Configure();
			foreach(var project in Projects) {
				project.ConfigureAll();
			}
		}

		public void AddTask(string name, TaskBase task) {
			task.Name = name;
			task.Project = this;
			Tasks.Add(task);
		}

		public IFile File(string path) {
			return fileSystem.File(path);
		}

		public string Name {
			get;
		}

		public string PathPrefix {
			get;
		}

		public string PathDescription {
			get;
		}

		public TaskCollection Tasks { get; }

		public ProjectCollection Projects { get; }

		[Serializable]
		private class TaskRecord {
			private readonly TaskState input = new TaskState();
			private readonly TaskState output = new TaskState();

			public void RecordInputs(TaskBase task) {
				input.RecordFiles(task.InputFiles);
			}

			public void RecordOutputs(TaskBase task) {
				output.RecordFiles(task.OutputFiles);
			}

			public bool IsUpToDate(TaskBase task) {
				var hasOutputs = task.OutputFiles.Any();
				var inputsUpToDate = input.UpToDate(task.InputFiles);
				var outputsUpToDate = output.UpToDate(task.OutputFiles);
				return hasOutputs && inputsUpToDate && outputsUpToDate;
			}
		}

		[Serializable]
		private class TaskState {
			private readonly Dictionary<string, DateTimeOffset> fileStamps = new Dictionary<string, DateTimeOffset>();

			public void RecordFiles(IEnumerable<IFile> files) {
				foreach(var file in files) {
					fileStamps[file.FullPath] = file.LastWriteTimeUtc;
				}
			}

			public bool UpToDate(IEnumerable<IFile> files) {
				return ToHashSet(fileStamps.Select(x => new { Path = x.Key, TimeStamp = x.Value })).SetEquals(files.Select(x => new { Path = x.FullPath, TimeStamp = x.LastWriteTimeUtc }));
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
			return taskNamesToExecute.Select(GetTaskByName).ToArray();
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
			return records.TryGetValue(task.Name, out var oldRecord) && oldRecord.IsUpToDate(task);
		}

		private void SaveTaskState(TaskBase task, TaskRecord record) {
			records[task.Name] = record;
			taskRecordCache.CreateDirectories();
			taskRecordCache.WriteAll(records);
		}

		private TaskBase GetTaskByName(string name) {
			var path = name.Split(':');
			return GetTaskByPath(new Queue<string>(path));
		}

		private TaskBase GetTaskByPath(Queue<string> path) {
			var pathPart = path.Dequeue();
			return path.Count > 0
				? Projects[pathPart].GetTaskByPath(path)
				: Tasks[pathPart];
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
