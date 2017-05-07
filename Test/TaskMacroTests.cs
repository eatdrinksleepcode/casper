using NUnit.Framework;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class TaskMacroTests {

		private RedirectedStandardOutput output;
		private IFileSystem fileSystem;

		[SetUp]
		public void SetUp() {
			output = RedirectedStandardOutput.RedirectOut();
			fileSystem = new StubFileSystem();
		}

		[TearDown]
		public void TearDown() {
			output.Clear();
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
		Source: File('Source.txt'), 
		Destination: File('Destination.txt'))
";

			var project = LoadProject(scriptContents);
			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("copy", out task));
			Assert.IsInstanceOf<CopyFile>(task);

			CopyFile copyTask = (CopyFile)task;

			Assert.That(copyTask.Source.FullPath, Is.EqualTo("/Source.txt"));
			Assert.That(copyTask.Destination.FullPath, Is.EqualTo("/Destination.txt"));
		}

		private ProjectBase LoadProject(string scriptContents) {
			fileSystem.File("build.casper").WriteAllText(scriptContents);
			return BooProjectLoader.LoadProject("build.casper", fileSystem);
		}
	}
}
