using NUnit.Framework;
using System.IO;
using System;
using System.Collections.Generic;

namespace Casper {
	[TestFixture]
	public class ScriptTests {

		private StreamWriter standardOutWriter;
		private StreamReader standardOutReader;
		private MemoryStream standardOut;
		private TextWriter oldStandardOut;

		private List<string> scripts = new List<string>();

		[SetUp]
		public void SetUp() {
			Script.Reset();
			oldStandardOut = Console.Out;
			standardOut = new MemoryStream();
			standardOutWriter = new StreamWriter(standardOut) { AutoFlush = true };
			standardOutReader = new StreamReader(standardOut);
			Console.SetOut(standardOutWriter);
			scripts.Clear();
		}

		[TearDown]
		public void TearDown() {
			foreach (var script in scripts) {
				File.Delete(script);
			}
			Console.SetOut(oldStandardOut);
		}

		[Test]
		public void ExecuteTasksInOrder() {
			ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'

task goodbye:
	print 'Goodbye World!'
", "goodbye", "hello");
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExecuteTaskWithDependencyGraph() {
			ExecuteScript("Test1.casper", @"
task wake:
	print 'Stretch'

task shower(DependsOn: [wake]):
	print 'Squeaky clean'

task eat(DependsOn: [wake]):
	print 'Yummy!'

task dress(DependsOn: [shower]):
	print 'Dressed'

task leave(DependsOn: [dress, eat]):
	print 'Bye!'
", "leave");
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Stretch"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Squeaky clean"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Dressed"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Yummy!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Bye!"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExecuteTasksFromSubProject() {
			WriteScript("subprojectA\\test.casper", @"
task goodbye(DependsOn: [parent.GetTaskByName('hello')]):
	print 'Goodbye World!'
");
			ExecuteScript("test.casper", @"
task hello:
	print 'Hello World!'

include """"""subprojectA\test.casper""""""
", "goodbye");
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void TaskDoesNotExist() {
			Assert.Throws<CasperException>(() => ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'
", "hello", "goodbye"), "Task 'goodbye' does not exist");
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void CompilationFailure() {
			Assert.Throws<CasperException>(() => ExecuteScript("Test1.casper", "foobar", "hello"));
			Assert.That(standardOutReader.ReadLine(), Is.Null);
		}

		[Test]
		public void UnhandledException() {
			Assert.Throws<Exception>(() => ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")", "hello"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		void ExecuteScript(string scriptPath, string scriptContents, params string[] args) {
			WriteScript(scriptPath, scriptContents);
			Script.CompileAndExecuteTasks(scriptPath, args);
			standardOut.Seek(0, SeekOrigin.Begin);
		}

		void WriteScript(string scriptPath, string scriptContents) {
			scripts.Add(scriptPath);
			File.WriteAllText(scriptPath, scriptContents);
		}
	}
}
