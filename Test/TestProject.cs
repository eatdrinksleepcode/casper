using Casper.IO;

namespace Casper {
	public class TestProject : ProjectBase {

		public TestProject(IFileSystem fileSystem) : this("Root", fileSystem) {}

		public TestProject(IFileSystem fileSystem, IDirectory location) : base(fileSystem, location.FullPath) {
		}

		public TestProject(string name, IFileSystem fileSystem) : base(fileSystem, ".", name) {
		}

		public TestProject(ProjectBase parent, string name) : base(parent, ".", name) {
		}

		public TestProject(ProjectBase parent, IDirectory location, string name) : base(parent, location.FullPath, name) {
		}

		public void ExecuteTasks(params string[] taskNamesToExecute) {
			var taskGraph = BuildTaskExecutionGraph(taskNamesToExecute);
			taskGraph.ExecuteTasks();
		}

		public TaskExecutionGraph BuildTaskExecutionGraph(params string[] taskNamesToExecute) {
			return base.BuildTaskExecutionGraph(taskNamesToExecute);
		}

		public TestTask AddTestTask(string taskName, params TestTask[] dependencies) {
			var csharpCompile = new TestTask(dependencies);
			AddTask(taskName, csharpCompile);
			return csharpCompile;
		}
	}
}
