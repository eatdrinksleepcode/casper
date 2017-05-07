using System.Linq;
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
				Source = sourceFile,
				Destination = destinationFile,
			};
			copyTask.Execute(fileSystem);
			Assert.True(destinationFile.Exists());
			Assert.That(destinationFile.ReadAllText(), Is.EqualTo("Hello World!"));
		}

		[Test]
		public void InputAndOutputFiles() {
			var sourceFile = fileSystem.File("Source.txt");
			sourceFile.WriteAllText("Hello World!");
			var destinationFile = fileSystem.File("Destination.txt");
			var copyTask = new CopyFile {
				Source = sourceFile,
				Destination = destinationFile,
			};
			Assert.That(copyTask.InputFiles.Count(), Is.EqualTo(1));
			Assert.That(copyTask.InputFiles.First().FullPath, Is.EqualTo("/Source.txt"));
			Assert.That(copyTask.OutputFiles.Count(), Is.EqualTo(1));
			Assert.That(copyTask.OutputFiles.First().FullPath, Is.EqualTo("/Destination.txt"));
		}

		[Test]
		public void MissingSource() {
			var copyTask = new CopyFile {
				Destination = fileSystem.File("Destination.txt"),
			};

			Assert.Throws<CasperException>(() => copyTask.Execute(fileSystem));
		}

		[Test]
		public void MissingDestination() {
			var copyTask = new CopyFile {
				Source = fileSystem.File("Source.txt"),
			};

			Assert.Throws<CasperException>(() => copyTask.Execute(fileSystem));
		}
	}
}
