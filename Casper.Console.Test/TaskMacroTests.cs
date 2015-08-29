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
			Script.Reset();
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
		}
		
		[Test]
		public void MakeTask() {

			string scriptContents = @"
task hello:
	print 'Hello World!'
";
			File.WriteAllText("Test1.casper", scriptContents);
			Script.CompileAndExecute("Test1.casper", "hello");

			standardOutWriter.Flush();
			standardOutStream.Seek(0, SeekOrigin.Begin);
			var standardOut = new StreamReader(standardOutStream);
			Assert.That(standardOut.ReadLine(), Is.EqualTo("Hello World!"));
		}
	}
}

