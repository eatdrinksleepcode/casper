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
			File.Delete("Test1.casper");
			Console.SetOut(oldStandardOut);
			standardOutStream.Dispose();
			standardOutStream = null;
			standardOutWriter = null;

			File.Delete("Source.txt");
			File.Delete("Destination.txt");
		}
		
		[Test]
		public void MakeTask() {

			string scriptContents = @"
task hello:
	print 'Hello World!'
";
			File.WriteAllText("Test1.casper", scriptContents);

			var project = BooProject.LoadProject("Test1.casper");
			project.ExecuteTasks(new [] { "hello" });

			standardOutWriter.Flush();
			standardOutStream.Seek(0, SeekOrigin.Begin);
			var standardOut = new StreamReader(standardOutStream);
			Assert.That(standardOut.ReadLine(), Is.EqualTo("hello:"));
			Assert.That(standardOut.ReadLine(), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void MakeTypedTask() {

			string scriptContents = @"
import Casper
task copy(CopyFile,
		Source: 'Source.txt', 
		Destination: 'Destination.txt')
";
			var destinationFileName = "Destination.txt";

			File.WriteAllText("Test1.casper", scriptContents);
			File.WriteAllText("Source.txt", "Hello World!");
			File.Delete(destinationFileName);
			Assert.False(File.Exists(destinationFileName));

			var project = BooProject.LoadProject("Test1.casper");
			project.ExecuteTasks(new [] { "copy" });

			Assert.True(File.Exists(destinationFileName));
			Assert.That(File.ReadAllText(destinationFileName), Is.EqualTo("Hello World!"));
		}
	}
}
