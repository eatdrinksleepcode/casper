using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class ExecTests {
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
