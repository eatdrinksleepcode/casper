using System;
using System.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class BooProjectLoaderTests {

		StreamWriter standardOutWriter;
		StreamReader standardOutReader;
		MemoryStream standardOut;
		TextWriter oldStandardOut;

		[SetUp]
		public void SetUp() {
			oldStandardOut = Console.Out;
			standardOut = new MemoryStream();
			standardOutWriter = new StreamWriter(standardOut) { AutoFlush = true };
			standardOutReader = new StreamReader(standardOut);
			Console.SetOut(standardOutWriter);
		}

		[TearDown]
		public void TearDown() {
			Console.SetOut(oldStandardOut);
		}

		[Test]
		public void CompilationFailure() {
			Assert.Throws<CasperException>(() => ExecuteScript("foobar", "hello"));
			Assert.That(standardOutReader.ReadLine(), Is.Null);
		}

		[Test]
		public void UnhandledExceptionDuringConfiguration() {
			Assert.Throws<InvalidOperationException>(() => ExecuteScript(@"raise System.InvalidOperationException(""Script failure"")", "hello"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void UnhandledExceptionDuringExecution() {
			var ex = Assert.Throws<Exception>(() => ExecuteScript(@"
task hello:
	raise System.Exception(""Task failure"")
", "hello"));
			Assert.That(ex.GetType(), Is.EqualTo(typeof(Exception)));
			Assert.That(ex.Message, Is.EqualTo("Task failure"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		void ExecuteScript(string scriptContents, params string[] args) {
			var project = BooProjectLoader.LoadProject(new StringReader(scriptContents));
			project.ExecuteTasks(args);
			standardOut.Seek(0, SeekOrigin.Begin);
		}
	}
}
