using System;
using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class TaskMacroTests {

		private RedirectedStandardOutput output;

		[SetUp]
		public void SetUp() {
			output = RedirectedStandardOutput.RedirectOut();
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

			var project = BooProjectLoader.LoadProject(new StringReader(scriptContents));

			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("hello", out task));
			Assert.IsInstanceOf<Task>(task);

			task.Execute();

			Assert.That(output.ToString(), Is.EqualTo("Hello World!\n"));
		}

		[Test]
		public void MakeTypedTask() {

			string scriptContents = @"
import System.IO
import Casper
task copy(CopyFile,
		Source: FileInfo('Source.txt'), 
		Destination: FileInfo('Destination.txt'))
";

			var project = BooProjectLoader.LoadProject(new StringReader(scriptContents));
			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("copy", out task));
			Assert.IsInstanceOf<CopyFile>(task);

			CopyFile copyTask = (CopyFile)task;

			Assert.That(copyTask.Source.Name, Is.EqualTo("Source.txt"));
			Assert.That(copyTask.Destination.Name, Is.EqualTo("Destination.txt"));
		}
	}
}
