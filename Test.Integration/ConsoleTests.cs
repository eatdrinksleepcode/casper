using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Casper {
	[TestFixture]
	public class ConsoleTests {
		List<string> scripts = new List<string>();

		StringReader standardOutput;
		StringReader standardError;

		[SetUp]
		public void SetUp() {
			scripts.Clear();
		}

		[TearDown]
		public void TearDown() {
			foreach (var script in scripts) {
				File.Delete(script);
			}
		}

		[Test]
		public void ExecuteTasksInOrder() {
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

			WriteScript("SubProject/test.casper", @"
");

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

		void WriteScript(string scriptName, string scriptContents) {
			scripts.Add(scriptName);
			var directory = Path.GetDirectoryName(scriptName);
			if(!string.IsNullOrEmpty(directory)) {
				Directory.CreateDirectory(directory);
			}
			File.WriteAllText(scriptName, scriptContents);
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
