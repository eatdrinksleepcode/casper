using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ExecTests {

		private static RedirectedStandardOutput output;
		private static RedirectedStandardOutput error;

		IFileSystem fileSystem = RealFileSystem.Instance;
		IDirectory workingDirectory;

		[TestFixtureSetUp]
		public static void OneTimeSetUp() {
			output = RedirectedStandardOutput.RedirectOut();
			error = RedirectedStandardOutput.RedirectError();
		}

		[SetUp]
		public void SetUp() {
			output.Clear();
			error.Clear();
			workingDirectory = fileSystem.MakeTemporaryDirectory();
		}

		[TearDown]
		public void TearDown() {
			workingDirectory.Delete();
		}

		[TestFixtureTearDown]
		public void OneTimeTearDown() {
			output.Dispose();
			error.Dispose();
		}

		[Test]
		public void HideConsoleOutput() {
			var task = new Exec {
				Executable = "echo",
				Arguments = "'Hello World!'",
			};
			task.Execute(fileSystem);

			Assert.That(output.ToString(), Is.Null.Or.Empty);
		}

		[Test]
		public void ShowConsoleOutput() {
			var task = new Exec {
				Executable = "echo",
				Arguments = "Hello World!",
				ShowOutput = true,
			};
			task.Execute(fileSystem);

			Assert.That(output.Read().ReadToEnd().TrimEnd(), Does.EndWith("Hello World!"));
		}

		[Test]
		public void DontDupeConsoleOutputOnFailure() {
			var fooFile = workingDirectory.File("foo.txt");
			var barFile = workingDirectory.File("bar.txt");
			fooFile.Delete();
			barFile.Delete();

			var task = new Exec {
				WorkingDirectory = workingDirectory.FullPath,
				Executable = MoveCommand,
				Arguments = "foo.txt bar.txt",
				ShowOutput = true,
			};

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));

			var errorContent = error.ToString();
			var errorStart = errorContent.Substring(0, 10);
			
			Assert.That(errorContent.Substring(10), Is.Not.StringContaining(errorStart));
		}

		[Test]
		public void ExecAndArguments() {
			var fooFile = workingDirectory.File("foo.txt");
			var barFile = workingDirectory.File("bar.txt");

			fooFile.WriteAllText("Hello World!");
			barFile.Delete();

			var task = new Exec {
				WorkingDirectory = workingDirectory.FullPath,
				Executable = MoveCommand,
				Arguments = "foo.txt bar.txt",
			};
			task.Execute(fileSystem);

			Assert.False(fooFile.Exists());
			Assert.True(barFile.Exists());
			Assert.That(barFile.ReadAllText(), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void Fail() {
			var fooFile = workingDirectory.File("foo.txt");
			var barFile = workingDirectory.File("bar.txt");
			fooFile.Delete();
			barFile.Delete();

			var task = new Exec {
				WorkingDirectory = workingDirectory.FullPath,
				Executable = MoveCommand,
				Arguments = "foo.txt bar.txt",
			};

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));
			Assert.False(fooFile.Exists());
			Assert.False(barFile.Exists());

			Assert.That(error.ToString(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void MissingExecutable() {
			var task = new Exec {
				WorkingDirectory = workingDirectory.FullPath,
				Arguments = "foo.txt bar.txt",
			};

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));
		}

		string MoveCommand {
			get { return Environment.IsUnix ? "mv" : "move"; }
		}
	}
}
