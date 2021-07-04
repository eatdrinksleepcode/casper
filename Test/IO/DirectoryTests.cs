using System.Reflection;
using NUnit.Framework;
using static System.IO.Path;

namespace Casper.IO {
	public abstract class DirectoryTests {
		private IFileSystem fileSystem;
		private IDirectory currentDirectory;
		private IDirectory originalWorkingDirectory;

		[OneTimeSetUp]
		public void SetUpOnce() {
			fileSystem = GetFileSystemInstance();
			var testParentDirectory = fileSystem.File(Assembly.GetExecutingAssembly().Location).Directory;
			currentDirectory = testParentDirectory.Directory(typeof(FileTests).Name);
			currentDirectory.Delete();
			currentDirectory.Create();
		}

		[SetUp]
		public void SetUp() {
			originalWorkingDirectory = fileSystem.GetCurrentDirectory();
			currentDirectory.SetAsCurrent();
		}

		[TearDown]
		public void TearDown() {
			originalWorkingDirectory.SetAsCurrent();
		}

		protected abstract IFileSystem GetFileSystemInstance();

		[Test]
		public void RootDirectory() {
			if(Environment.IsUnixFileSystem) {
				Assert.That(currentDirectory.RootDirectory.FullPath, Is.EqualTo("/"));
			} else {
				Assert.Inconclusive("Can't predetermine root path on Windows");
			}
		}

		[Test]
		public void RelativeDirectory() {
			const string directoryName = nameof(RelativeDirectory);
			var directory = fileSystem.Directory(directoryName);
			Assert.That(directory.Name, Is.EqualTo(directoryName));
			Assert.That(directory.FullPath, Is.EqualTo(Combine(currentDirectory.FullPath, directoryName)));
		}

		[Test]
		public void AbsoluteDirectory() {
			var rootPath = currentDirectory.RootDirectory.FullPath;
			const string directoryName = nameof(AbsoluteDirectory);
			var directoryPath = Combine(rootPath, directoryName);
			var directory = fileSystem.Directory(directoryPath);
			Assert.That(directory.Name, Is.EqualTo(directoryName));
			Assert.That(directory.FullPath, Is.EqualTo(Combine(rootPath, directoryPath)));
		}

		[Test]
		public void CreateDelete() {
			var testDirectory = currentDirectory.Directory(nameof(CreateDelete));
			testDirectory.Delete();

			testDirectory.Delete();
			Assert.False(testDirectory.Exists());

			testDirectory.Create();
			Assert.True(testDirectory.Exists());

			testDirectory.Delete();
			Assert.False(testDirectory.Exists());
		}
	}
}
