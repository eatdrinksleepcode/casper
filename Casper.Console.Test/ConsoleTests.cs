using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Casper {
	[TestFixture]
	public class ConsoleTests {
		[Test]
		public void ExecuteTask() {
			var testProcess = ExecuteScript("Test1.casper", @"
import Casper.Script
task 'hello':
	print 'Hello World!'

task 'goodbye':
	print 'Goodbye World!'
", "goodbye");
			Assert.That(testProcess.StandardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.EqualTo("Goodbye World!"));
		}

		[Test]
		public void TaskDoesNotExist() {
			var testProcess = ExecuteScript("Test1.casper", @"
import Casper.Script
task 'hello':
	print 'Hello World!'
", "goodbye");
			Assert.That(testProcess.StandardError.ReadLine(), Is.EqualTo("Task 'goodbye' does not exist"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(2));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.Null);
		}

		[Test]
		public void CompilationFailure() {
			var testProcess = ExecuteScript("Test1.casper", "foobar", "hello");
			Assert.That(testProcess.StandardError.ReadToEnd(), Is.Not.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(1));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.Null);
		}

		[Test]
		public void UnhandledException() {
			var testProcess = ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")", "hello");
			Assert.That(testProcess.StandardError.ReadLine(), Is.EqualTo("System.Exception: Script failure"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(255));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.Null);
		}

		static Process ExecuteScript(string scriptName, string scriptContents, params string[] args) {
			Process testProcess;
			try {
				File.WriteAllText(scriptName, scriptContents);
				testProcess = Process.Start(new ProcessStartInfo {
					FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "casper.exe"),
					Arguments = scriptName + " " + string.Join(" ", args),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				});
				testProcess.WaitForExit();
			}
			finally {
				File.Delete(scriptName);
			}
			return testProcess;
		}
	}
}

