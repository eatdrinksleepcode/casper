using NUnit.Framework;
using System.IO;
using System;

namespace Casper {
	[TestFixture]
	public class ScriptTests {

		private StreamWriter standardOutWriter;
		private StreamReader standardOutReader;
		private MemoryStream standardOut;
		private TextWriter oldStandardOut;

		[SetUp]
		public void SetUp() {
			Script.Reset();
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
		public void ExecuteTasksInOrder() {
			ExecuteScript("Test1.casper", @"
import Casper.Script
task 'hello':
	act:
		print 'Hello World!'

task 'goodbye':
	act:
		print 'Goodbye World!'
", "goodbye", "hello");
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExecuteTaskWithDependencyGraph() {
			ExecuteScript("Test1.casper", @"
import Casper.Script

wake = task('wake'):
	act:
		print 'Stretch'

shower = task('shower'):
	dependsOn wake
	act:
		print 'Squeaky clean'

eat = task('eat'):
	dependsOn wake
	act:
		print 'Yummy!'

dress = task('dress'):
	dependsOn shower
	act:
		print 'Dressed'

task 'leave':
	dependsOn dress
	dependsOn eat
	act:
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
		public void TaskDoesNotExist() {
			Assert.Throws<CasperException>(() => ExecuteScript("Test1.casper", @"
import Casper.Script
task 'hello':
	act:
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

		void ExecuteScript(string scriptName, string scriptContents, params string[] args) {
			try {
				File.WriteAllText(scriptName, scriptContents);
				Script.CompileAndExecute(scriptName, args);
				standardOut.Seek(0, SeekOrigin.Begin);
			} finally {
				File.Delete(scriptName);
			}
		}
	}
}
