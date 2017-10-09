using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class ConsoleTests {
		IDirectory workingDirectory;

		StringReader standardOutput;
		StringReader standardError;

		[SetUp]
		public void SetUp() {
			workingDirectory = RealFileSystem.Instance.MakeTemporaryDirectory();
		}

		[TearDown]
		public void TearDown() {
			workingDirectory.Delete();
		}

		[Test]
		public void ExecuteTasksWithOutputInOrder() {
			var testProcess = ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'

task goodbye:
	print 'Goodbye World!'
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
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
			var testProcess = ExecuteScript("Test1.casper", @"
task hello
task goodbye
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
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
			var testProcess = ExecuteScript("Test1.casper", @"
task hello:
	System.Console.Write(""Hello World!"");
", "goodbye", "hello");
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
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
			var testProcess = ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'
", "hello", "goodbye");
			Assert.That(standardError.ReadLine(), Is.Empty);
			Assert.That(standardError.ReadLine(), Is.EqualTo("BUILD FAILURE"));
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("* What went wrong:"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("Task 'goodbye' does not exist in root project"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(2));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void ExceptionDuringConfiguration() {
			var testProcess = ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")", "hello");
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("BUILD FAILURE"));
			Assert.That(standardError.ReadLine(), Is.EqualTo(""));
			Assert.That(standardError.ReadLine(), Is.EqualTo("* What went wrong:"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("System.Exception: Script failure"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(255));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void TaskFailure() {
			File.Delete("foo.txt");
			var testProcess = ExecuteScript("Test1.casper", @"
import Casper;
task move(Exec, Executable: 'mv', Arguments: 'foo.txt bar.txt')
", "move");
			Assert.That(standardError.ReadToEnd(), Is.Not.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(CasperException.EXIT_CODE_TASK_FAILED));
			Assert.That(standardOutput.ReadLine(), Is.EqualTo(":move"));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Help() {
			var testProcess = ExecuteCasper("--help");
			Assert.That(standardError.ReadToEnd(), Contains.Substring("USAGE:"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
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
			var testProcess = ExecuteCasper("test.casper --tasks");
			Assert.That(standardError.ReadLine(), Is.EqualTo("hello - Hello"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("goodbye - Goodbye"));
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Projects() {
			WriteScript("test.casper", @"
include 'SubProject/test.casper'
");

			WriteScript("test.casper", @"
", "SubProject");

			var testProcess = ExecuteCasper("test.casper --projects");
			Assert.That(standardOutput.ReadLine(), Is.Empty);
			Assert.That(standardOutput.ReadLine(), Does.StartWith("Total time: "));
			Assert.That(standardOutput.ReadToEnd(), Is.Empty);
			Assert.That(standardError.ReadLine(), Is.EqualTo("root project"));
			Assert.That(standardError.ReadLine(), Is.EqualTo("project ':SubProject'"));
			Assert.That(standardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
		}

		Process ExecuteScript(string scriptName, string scriptContents, params string[] args) {
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

		Process ExecuteCasper(string arguments) {
			var testProcess = new Process();
			var standardOutBuilder = new StringBuilder();
			var standardErrorBuilder = new StringBuilder();
			var combinedOutBuilder = new StringBuilder();
			testProcess.OutputDataReceived += (sender, e) => {
				if(null != e.Data) {
					standardOutBuilder.AppendLine(e.Data);
					combinedOutBuilder.AppendLine(e.Data);
				}
			};
			testProcess.ErrorDataReceived += (sender, e) => {
				if(null != e.Data) {
					standardErrorBuilder.AppendLine(e.Data);
					combinedOutBuilder.AppendLine(e.Data);
				}
			};
			testProcess.StartInfo = new ProcessStartInfo {
				FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "casper.exe"),
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = workingDirectory.FullPath,
			};
			testProcess.Start();
			testProcess.BeginErrorReadLine();
			testProcess.BeginOutputReadLine();
			testProcess.WaitForExit();
			System.Console.Write(combinedOutBuilder);
			standardOutput = new StringReader(standardOutBuilder.ToString());
			standardError = new StringReader(standardErrorBuilder.ToString());
			return testProcess;
		}
	}
}
