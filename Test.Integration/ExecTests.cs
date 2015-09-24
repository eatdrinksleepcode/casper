using NUnit.Framework;
using System.IO;
using System;

namespace Casper {
	[TestFixture]
	public class ExecTests {

		MemoryStream output;
		StreamReader outputReader;
		TextWriter originalOutput;
		TextWriter originalError;

		[SetUp]
		public void SetUp() {
			output = new MemoryStream();
			outputReader = new StreamReader(output);
			
			originalOutput = Console.Out;
			originalError = Console.Error;

			Console.SetOut(new StreamWriter(output) { AutoFlush = true });
			Console.SetError(new StreamWriter(output) { AutoFlush = true });
		}

		[TearDown]
		public void TearDown() {
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
		}

		[Test]
		public void HideConsoleOutput() {
			var task = new Exec {
				Executable = "echo",
				Arguments = "'Hello World!'",
			};
			task.Execute();

			output.Seek(0, SeekOrigin.Begin);
			Assert.That(outputReader.ReadLine(), Is.Null.Or.Empty);
		}

		[Test]
		public void ExecAndArguments() {
			File.WriteAllText(@"foo.txt", "Hello World!");
			File.Delete("bar.txt");

			var task = new Exec {
				Executable = "mv",
				Arguments = "foo.txt bar.txt",
			};
			task.Execute();

			Assert.False(File.Exists("foo.txt"));
			Assert.True(File.Exists("bar.txt"));
			Assert.That(File.ReadAllText("bar.txt"), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void Fail() {
			File.Delete("foo.txt");
			File.Delete("bar.txt");

			var task = new Exec {
				Executable = "mv",
				Arguments = "foo.txt bar.txt",
			};
				
			Assert.Throws<CasperException>(() => task.Execute());
			Assert.False(File.Exists("foo.txt"));
			Assert.False(File.Exists("bar.txt"));

			output.Seek(0, SeekOrigin.Begin);
			Assert.That(outputReader.ReadLine(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void MissingExecutable() {
			var task = new Exec {
				Arguments = "foo.txt bar.txt",
			};

			Assert.Throws<CasperException>(() => task.Execute());
		}
	}
}
