using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class CopyFileTests {

		private IFileSystem fileSystem;

		[SetUp]
		public void SetUp() {
			fileSystem = new StubFileSystem();
		}

		[Test]
		public void CopyFile() {
			var sourceFile = fileSystem.File("Source.txt");
			sourceFile.WriteAllText("Hello World!");
			var destinationFile = fileSystem.File("Destination.txt");
			var copyTask = new CopyFile {
				Source = sourceFile.Path,
				Destination = destinationFile.Path,
			};
			copyTask.Execute(fileSystem);
			Assert.True(destinationFile.Exists());
			Assert.That(destinationFile.ReadAllText(), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void MissingSource() {
			var copyTask = new CopyFile {
				Destination = "Destination.txt",
			};

			Assert.Throws<CasperException>(() => copyTask.Execute(fileSystem));
		}

		[Test]
		public void MissingDestination() {
			var copyTask = new CopyFile {
				Source = "Source.txt",
			};

			Assert.Throws<CasperException>(() => copyTask.Execute(fileSystem));
		}
	}
}
