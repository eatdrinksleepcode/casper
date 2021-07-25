using System;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class FSharpProjectLoaderTests {

		private static RedirectedStandardOutput output;
		private IFileSystem fileSystem;

		[OneTimeSetUp]
		public static void OneTimeSetUp() {
			output = RedirectedStandardOutput.RedirectOut();
		}

		[SetUp]
		public void SetUp() {
			output.Clear();
			fileSystem = new StubFileSystem();
		}

		[OneTimeTearDown]
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
			var ex = Assert.Throws<InvalidOperationException>(() => ExecuteScript(@"invalidOp ""Script failure""", "hello"), "Script failure");
			Assert.That(output.ToString(), Is.Empty);
		}

		[Test]
		public void UnhandledExceptionDuringExecution() {
			var ex = Assert.Throws<Exception>(() => ExecuteScript(@"
project.AddTask ""hello"" Task(fun -> failWith ""Task failure"")
", "hello"));
			Assert.That(ex.GetType(), Is.EqualTo(typeof(Exception)));
			Assert.That(ex.Message, Is.EqualTo("Task failure"));
			Assert.That(output.ToString(), Is.EqualTo(":hello\n".NormalizeNewLines()));
		}

		void ExecuteScript(string scriptContents, params string[] args) {
			fileSystem.File("build.casper.fsx").WriteAllText(scriptContents);
			var project = new FSharpProjectLoader(fileSystem, "build.casper.fsx").LoadProject(".");
			var taskGraph = project.BuildTaskExecutionGraph(args);
			taskGraph.ExecuteTasks();
		}
	}
}
