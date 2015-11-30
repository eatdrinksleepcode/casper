using System;
using System.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class BooProjectLoaderTests {

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
		public void CompilationFailure() {
			Assert.Throws<CasperException>(() => ExecuteScript("foobar", "hello"));
			Assert.That(output.ToString(), Is.Empty);
		}

		[Test]
		public void UnhandledExceptionDuringConfiguration() {
			Assert.Throws<InvalidOperationException>(() => ExecuteScript(@"raise System.InvalidOperationException(""Script failure"")", "hello"));
			Assert.That(output.ToString(), Is.Empty);
		}

		[Test]
		public void UnhandledExceptionDuringExecution() {
			var ex = Assert.Throws<Exception>(() => ExecuteScript(@"
task hello:
	raise System.Exception(""Task failure"")
", "hello"));
			Assert.That(ex.GetType(), Is.EqualTo(typeof(Exception)));
			Assert.That(ex.Message, Is.EqualTo("Task failure"));
			Assert.That(output.ToString(), Is.EqualTo("hello\n"));
		}

		void ExecuteScript(string scriptContents, params string[] args) {
			var project = BooProjectLoader.LoadProject(new StringReader(scriptContents));
			project.ExecuteTasks(args);
		}
	}
}
