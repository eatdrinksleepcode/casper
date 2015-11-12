using System;
using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class TaskMacroTests {

		private TextWriter oldStandardOut;
		private MemoryStream standardOutStream;
		private TextWriter standardOutWriter;

		[SetUp]
		public void SetUp() {
			oldStandardOut = Console.Out;
			standardOutStream = new MemoryStream();
			standardOutWriter = new StreamWriter(standardOutStream);
			Console.SetOut(standardOutWriter);
		}

		[TearDown]
		public void TearDown() {
			Console.SetOut(oldStandardOut);
			standardOutStream.Dispose();
			standardOutStream = null;
			standardOutWriter = null;
		}
		
		[Test]
		public void MakeTask() {

			// TODO: use project properties instead of console output to test task execution
			string scriptContents = @"
task hello:
	print 'Hello World!'
";

			var project = BooProject.LoadProject(new StringReader(scriptContents));

			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("hello", out task));
			Assert.IsInstanceOf<Task>(task);

			task.Execute();

			standardOutWriter.Flush();
			standardOutStream.Seek(0, SeekOrigin.Begin);
			var standardOut = new StreamReader(standardOutStream);
			Assert.That(standardOut.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.True(standardOut.EndOfStream);
		}

		[Test]
		public void MakeTypedTask() {

			string scriptContents = @"
import Casper
task copy(CopyFile,
		Source: 'Source.txt', 
		Destination: 'Destination.txt')
";

			var project = BooProject.LoadProject(new StringReader(scriptContents));
			TaskBase task;
			Assert.True(project.Tasks.TryGetValue("copy", out task));
			Assert.IsInstanceOf<CopyFile>(task);

			CopyFile copyTask = (CopyFile)task;

			Assert.That(copyTask.Source, Is.EqualTo("Source.txt"));
			Assert.That(copyTask.Destination, Is.EqualTo("Destination.txt"));
		}
	}
}
