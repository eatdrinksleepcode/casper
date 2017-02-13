using System;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	public class TaskExecutionGraphTests : IDisposable {
		private RedirectedStandardOutput output;
		private IFileSystem fileSystem;
		private IFile outputFile;

		[SetUp]
		public void SetUp() {
			output = RedirectedStandardOutput.RedirectOut();
			fileSystem = new StubFileSystem();
			outputFile = fileSystem.File("output.txt");
			outputFile.WriteAllText("Output");
		}

		[TearDown]
		public void TearDown() {
			output.Clear();
		}

		void IDisposable.Dispose() {
			output.Dispose();
		}

		[Test]
		public void PrintTasks() {
			var project = new TestProject(fileSystem);
			var task1 = new TestTask();
			project.AddTask("task1", task1);
			var task2 = new TestTask();
			project.AddTask("task2", task2);

			var graph = new TaskExecutionGraph(task1, task2);

			graph.ExecuteTasks();

			Assert.That(output.ToString(), Is.EqualTo(":task1\n:task2\n".NormalizeNewLines()));
		}

		[Test]
		public void PrintTaskAsUpToDate() {
			var project = new TestProject(fileSystem);
			var task1 = new TestTask();
			task1.AddOutput(outputFile);
			project.AddTask("task1", task1);

			var graph = new TaskExecutionGraph(task1);

			graph.ExecuteTasks();
			Assert.That(output.ToString(), Is.EqualTo(":task1\n".NormalizeNewLines()));

			output.Clear();

			graph.ExecuteTasks();
			Assert.That(output.ToString(), Is.EqualTo(":task1 (UP-TO-DATE)\n".NormalizeNewLines()));
		}
	}
}
