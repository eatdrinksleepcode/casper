using NUnit.Framework;
using System.IO;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class FSharpConsoleTests {
		IDirectory originalWorkingDirectory;
		IDirectory workingDirectory;

		RedirectedStandardOutput output;
		RedirectedStandardOutput error;
		TextReader standardOutput;
		TextReader standardError;

		[SetUp]
		public void SetUp() {
			output = RedirectedStandardOutput.RedirectOut();
			error = RedirectedStandardOutput.RedirectError();
			
			workingDirectory = RealFileSystem.Instance.MakeTemporaryDirectory();
			originalWorkingDirectory = RealFileSystem.Instance.GetCurrentDirectory();
			workingDirectory.SetAsCurrent();
		}

		[TearDown]
		public void TearDown() {
			output.Dispose();
			error.Dispose();
			
			originalWorkingDirectory.SetAsCurrent();
			workingDirectory.Delete();
		}

		[Test]
		public void ExecuteTasksWithOutputInOrder() {
			var exitCode = ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'

task goodbye:
	print 'Goodbye World!'
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(exitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":goodbye"));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":hello"));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("BUILD SUCCESS"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		[Ignore("Use pre-write spacing instead of post-write spacing")]
		public void ExecuteSimpleTasksInOrder() {
			var exitCode = ExecuteScript("Test1.casper", @"
task hello
task goodbye
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(exitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":goodbye"));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":hello"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("BUILD SUCCESS"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		[Ignore("Detect incomplete task writes")]
		public void TaskWithIncompleteOutput() {
			var exitCode = ExecuteScript("Test1.casper", @"
task hello:
	System.Console.Write(""Hello World!"");
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(exitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":hello"));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Is.EqualTo("BUILD SUCCESS"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void TaskDoesNotExist() {
			var exitCode = ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'
", "hello", "goodbye");
			Assert.That(standardError.ReadLine(), Is.Empty);
			Assert.That(standardError.ReadLine(), Is.EqualTo("BUILD FAILURE"));
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("* What went wrong:"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("Task 'goodbye' does not exist in root project"));
			Assert.That(exitCode, Is.EqualTo(2));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExceptionDuringConfiguration() {
			var exitCode = ExecuteScript("Test1.casper.fsx", @"failWith ""Script failure""", "hello");
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("BUILD FAILURE"));
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("* What went wrong:"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("System.Exception: Script failure"));
			Assert.That(exitCode, Is.EqualTo(255));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void TaskFailure() {
			File.Delete("foo.txt");
			var exitCode = ExecuteScript("Test1.casper", @"
import Casper;
task move(Exec, Executable: 'mv', Arguments: 'foo.txt bar.txt')
", "move");
			Assert.That(standardError.ReadToEnd(), Is.Not.Empty);
			Assert.That(exitCode, Is.EqualTo((int)CasperException.KnownExitCode.TaskFailed));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":move"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Help() {
			var exitCode = ExecuteCasper("--help");
			Assert.That(standardError.ReadToEnd(), Contains.Substring("USAGE:"));
			Assert.That(exitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Tasks() {
			WriteScript("test.casper", @"
task hello(Description: 'Hello'):
	print 'Hello World!'

task goodbye(Description: 'Goodbye'):
	print 'Goodbye World!'
");
			var exitCode = ExecuteCasper("test.casper --tasks");
			Assert.That(standardError.ReadLine(), Is.EqualTo("hello - Hello"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("goodbye - Goodbye"));
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(exitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Projects() {
			WriteScript("test.casper", @"
include 'SubProject'
");

			WriteScript("test.casper", @"
", "SubProject");

			var exitCode = ExecuteCasper("test.casper --projects");
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
			Assert.That(standardError.ReadLine(), Is.EqualTo("root project"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("project ':SubProject'"));
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(exitCode, Is.EqualTo(0));
		}

		[Test]
		public void AbsoluteScriptPath() {
			WriteScript("test.casper", @"
");

			var exitCode = ExecuteCasper(workingDirectory.File("test1.casper").FullPath);

			Assert.That(standardError.ReadLine(), Is.Empty);
			Assert.That(standardError.ReadLine(), Is.EqualTo("BUILD FAILURE"));
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("* What went wrong:"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("ScriptFile must be a relative path"));
			Assert.That(exitCode, Is.EqualTo(5));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		int ExecuteScript(string scriptName, string scriptContents, params string[] args) {
			WriteScript(scriptName, scriptContents);
			var arguments = scriptName + " " + string.Join(" ", args);
			return ExecuteCasper(arguments);
		}

		void WriteScript(string scriptName, string scriptContents, string subDirectory = null) {
			var projectDir = workingDirectory;
			if(null != subDirectory) {
				projectDir = projectDir.Directory(subDirectory);
				projectDir.Create();
			}
			var scriptFile = projectDir.File(scriptName);
			scriptFile.WriteAllText(scriptContents);
		}

		int ExecuteCasper(string arguments) {
			var exitCode = MainClass.Main(arguments.Split(' '));
			standardOutput = output.Read();
			standardError = error.Read();
			return exitCode;
		}
	}
}
