using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class BooProjectTests {

		StreamWriter standardOutWriter;
		StreamReader standardOutReader;
		MemoryStream standardOut;
		TextWriter oldStandardOut;

		List<string> testFiles = new List<string>();

		[SetUp]
		public void SetUp() {
			oldStandardOut = Console.Out;
			standardOut = new MemoryStream();
			standardOutWriter = new StreamWriter(standardOut) { AutoFlush = true };
			standardOutReader = new StreamReader(standardOut);
			Console.SetOut(standardOutWriter);

			testFiles.Clear();
		}

		[TearDown]
		public void TearDown() {
			foreach (var script in testFiles) {
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
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("goodbye:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("hello:"));
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
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("wake:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Stretch"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("shower:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Squeaky clean"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("dress:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Dressed"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("eat:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Yummy!"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("leave:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Bye!"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExecuteTasksFromSubProject() {

			var subProjectDir = "subProjectA";
			var subProjectFile = subProjectDir.File("foo.txt");
			testFiles.Add(subProjectFile);

			if (Directory.Exists(subProjectDir)) {
				Directory.Delete(subProjectDir, true);
			}
			Directory.CreateDirectory(subProjectDir);
			File.Delete(subProjectFile);
			
			WriteScript(subProjectDir.File("test.casper"), @"
task goodbye(DependsOn: [parent.Tasks['hello']]):
	print System.IO.File.ReadAllText('foo.txt')
");
			ExecuteScript("test.casper", @"
task hello:
	System.IO.File.WriteAllText('" + subProjectDir.File("foo.txt") + @"', 'Hello World!')

include """"""" + subProjectDir.File("test.casper") + @"""""""
", "goodbye");

			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("hello:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("goodbye:"));
			Assert.That(standardOutReader.ReadLine(), Is.EqualTo("Hello World!"));
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
		public void UnhandledExceptionDuringConfiguration() {
			Assert.Throws<Exception>(() => ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")", "hello"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void UnhandledExceptionDuringExecution() {
			var ex = Assert.Throws<Exception>(() => ExecuteScript("Test1.casper", @"
task hello:
	raise System.Exception(""Task failure"")
", "hello"));
			Assert.That(ex.GetType(), Is.EqualTo(typeof(Exception)));
			Assert.That(ex.Message, Is.EqualTo("Task failure"));
			Assert.That(standardOutReader.ReadToEnd(), Is.Empty);
		}

		void ExecuteScript(string scriptPath, string scriptContents, params string[] args) {
			WriteScript(scriptPath, scriptContents);
			var project = BooProject.LoadProject(scriptPath);
			project.ExecuteTasks(args);
			standardOut.Seek(0, SeekOrigin.Begin);
		}

		void WriteScript(string scriptPath, string scriptContents) {
			testFiles.Add(scriptPath);
			File.WriteAllText(scriptPath, scriptContents);
		}
	}
}
