using System;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class BooProjectLoaderTests {

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
			Assert.That(output.ToString(), Is.EqualTo(":hello\n".NormalizeNewLines()));
		}

		void ExecuteScript(string scriptContents, params string[] args) {
			fileSystem.File("build.casper").WriteAllText(scriptContents);
			var project = new BooProjectLoader(fileSystem, "build.casper").LoadProject(".");
			var taskGraph = project.BuildTaskExecutionGraph(args);
			taskGraph.ExecuteTasks();
		}
	}
}
