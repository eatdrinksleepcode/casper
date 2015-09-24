using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class CopyFileTests {

		[TearDown]
		public void TearDown() {
			File.Delete("Source.txt");
			File.Delete("Destination.txt");
		}

		[Test]
		public void CopyFile() {
			File.WriteAllText("Source.txt", "Hello World!");
			var copyTask = new CopyFile {
				Source = "Source.txt",
				Destination = "Destination.txt",
			};

			File.Delete("Destination.txt");
			Assert.False(File.Exists("Destination.txt"));
			copyTask.Execute();
			Assert.True(File.Exists("Destination.txt"));
			Assert.That(File.ReadAllText("Destination.txt"), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void MissingSource() {
			var copyTask = new CopyFile {
				Destination = "Destination.txt",
			};

			Assert.Throws<CasperException>(() => copyTask.Execute());
		}

		[Test]
		public void MissingDestination() {
			var copyTask = new CopyFile {
				Source = "Source.txt",
			};

			Assert.Throws<CasperException>(() => copyTask.Execute());
		}
	}
}
