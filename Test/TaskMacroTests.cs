using NUnit.Framework;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class TaskMacroTests {

		private static RedirectedStandardOutput output;

		private IFileSystem fileSystem;

		[TestFixtureSetUp]
		public static void OneTimeSetUp() {
			output = RedirectedStandardOutput.RedirectOut();
		}

		[SetUp]
		public void SetUp() {
			output.Clear();
			fileSystem = new StubFileSystem();
		}

		[TestFixtureTearDown]
		public void OneTimeTearDown() {
			output.Dispose();
		}

		[Test]
		public void MakeTask() {

			// TODO: use project properties instead of console output to test task execution
			string scriptContents = @"
task hello:
	print 'Hello World!'
";
			ProjectBase project = LoadProject(scriptContents);

			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("hello", out task));
			Assert.IsInstanceOf<Task>(task);

			task.Execute(fileSystem);

			Assert.That(output.ToString(), Is.EqualTo("Hello World!\n".NormalizeNewLines()));
		}

		[Test]
		public void MakeTypedTask() {

			string scriptContents = @"
import System.IO
import Casper
task copy(CopyFile,
		Source: 'Source.txt', 
		Destination: 'Destination.txt')
";

			var project = LoadProject(scriptContents);
			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("copy", out task));
			Assert.IsInstanceOf<CopyFile>(task);

			CopyFile copyTask = (CopyFile)task;

			Assert.That(copyTask.Source, Is.EqualTo("Source.txt"));
			Assert.That(copyTask.Destination, Is.EqualTo("Destination.txt"));
		}

		private ProjectBase LoadProject(string scriptContents) {
			fileSystem.File("build.casper").WriteAllText(scriptContents);
			return new BooProjectLoader(fileSystem, "build.casper").LoadProject(".");
		}
	}
}
