using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

namespace Casper {
	[TestFixture]
	public class ConsoleTests {
		List<string> scripts = new List<string>();

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
			Assert.That(testProcess.StandardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.EqualTo("Goodbye World!"));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.EqualTo("Hello World!"));
			Assert.That(testProcess.StandardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void TaskDoesNotExist() {
			var testProcess = ExecuteScript("Test1.casper", @"
task hello:
	print 'Hello World!'
", "hello", "goodbye");
			Assert.That(testProcess.StandardError.ReadLine(), Is.EqualTo("Task 'goodbye' does not exist"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(2));
		}

		[Test]
		public void UnhandledException() {
			var testProcess = ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")", "hello");
			Assert.That(testProcess.StandardError.ReadLine(), Is.EqualTo("System.Exception: Script failure"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(255));
		}

		[Test]
		public void Help() {
			var testProcess = ExecuteCasper("--help");
			Assert.That(testProcess.StandardError.ReadToEnd(), Contains.Substring("USAGE:"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(testProcess.StandardOutput.ReadToEnd(), Is.Empty);
		}

		[Test]
		public void Tasks() {
			WriteScript("build.casper", @"
task hello:
	print 'Hello World!'

task goodbye:
	print 'Goodbye World!'
");
			var testProcess = ExecuteCasper("build.casper --tasks");
			var standardError = testProcess.StandardError.ReadToEnd();
			Assert.That(standardError, Contains.Substring("goodbye"));
			Assert.That(standardError, Contains.Substring("hello"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(testProcess.StandardOutput.ReadToEnd(), Is.Empty);
		}

		Process ExecuteScript(string scriptName, string scriptContents, params string[] args) {
			WriteScript(scriptName, scriptContents);
			var arguments = scriptName + " " + string.Join(" ", args);
			return ExecuteCasper(arguments);
		}

		void WriteScript(string scriptName, string scriptContents) {
			scripts.Add(scriptName);
			File.WriteAllText(scriptName, scriptContents);
		}

		Process ExecuteCasper(string arguments) {
			var testProcess = Process.Start(new ProcessStartInfo {
				FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "casper.exe"),
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			});
			testProcess.WaitForExit();
			return testProcess;
		}
	}
}
