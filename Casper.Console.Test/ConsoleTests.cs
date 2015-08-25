using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Casper.Console.Test {
	[TestFixture]
	public class ConsoleTests {
		[Test]
		public void ExecuteScript() {
			var testProcess = ExecuteScript("Test1.casper", "print 'Hello World!'");
			Assert.That(testProcess.StandardError.ReadToEnd(), Is.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(0));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void CompilationFailure() {
			var testProcess = ExecuteScript("Test1.casper", "foobar");
			Assert.That(testProcess.StandardError.ReadToEnd(), Is.Not.Empty);
			Assert.That(testProcess.ExitCode, Is.EqualTo(1));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.Null);
		}

		[Test]
		public void UnhandledException() {
			var testProcess = ExecuteScript("Test1.casper", @"raise System.Exception(""Script failure"")");
			Assert.That(testProcess.StandardError.ReadLine(), Is.EqualTo("System.Exception: Script failure"));
			Assert.That(testProcess.ExitCode, Is.EqualTo(255));
			Assert.That(testProcess.StandardOutput.ReadLine(), Is.Null);
		}

		static Process ExecuteScript(string scriptName, string scriptContents) {
			Process testProcess;
			try {
				File.WriteAllText(scriptName, scriptContents);
				testProcess = Process.Start(new ProcessStartInfo {
					FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "casper.exe"),
					Arguments = scriptName,
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

