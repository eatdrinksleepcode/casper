using System.Reflection;
using NUnit.Framework;
using static System.IO.Path;

namespace Casper.IO {
	public abstract class FileTests {

		private IFileSystem fileSystem;
		private IDirectory testDirectory;
		private IDirectory originalWorkingDirectory;

		[OneTimeSetUp]
		public void SetUpOnce() {
			fileSystem = GetFileSystemInstance();
			var testParentDirectory = fileSystem.File(Assembly.GetExecutingAssembly().Location).Directory;
			testDirectory = testParentDirectory.Directory(typeof(FileTests).Name);
			testDirectory.Delete();
			testDirectory.Create();
		}

		protected abstract IFileSystem GetFileSystemInstance();

		[OneTimeTearDown]
		public void TearDownOnce() {
			testDirectory.Delete();
		}

		[SetUp]
		public void SetUp() {
			originalWorkingDirectory = fileSystem.GetCurrentDirectory();
			testDirectory.SetAsCurrent();
		}

		[TearDown]
		public void TearDown() {
			originalWorkingDirectory.SetAsCurrent();
		}

		[Test]
		public void RelativeFile() {
			var fileName = $"${nameof(AbsoluteFile)}.file";
			var file = fileSystem.File(fileName);
			Assert.That(file.Name, Is.EqualTo(fileName));
			Assert.That(file.FullPath, Is.EqualTo(Combine(testDirectory.FullPath, fileName)));
			Assert.That(file.Directory.FullPath, Is.EqualTo(Combine(testDirectory.FullPath)));
		}

		[Test]
		public void AbsoluteFile() {
			var rootPath = testDirectory.RootDirectory.FullPath;
			var fileName = $"${nameof(AbsoluteFile)}.file";
			var filePath = Combine(rootPath, fileName);
			var file = fileSystem.File(filePath);
			Assert.That(file.Name, Is.EqualTo(fileName));
			Assert.That(file.FullPath, Is.EqualTo(Combine(rootPath, filePath)));
			Assert.That(file.Directory.FullPath, Is.EqualTo(rootPath));
		}

		[Test]
		public void CreateDelete() {
			var testFile = fileSystem.File($"{nameof(CreateDelete)}.test");

			Assert.That(testFile.Exists, Is.False);

			testFile.WriteAllText("foo");

			Assert.That(testFile.Exists, Is.True);

			testFile.Delete();

			Assert.That(testFile.Exists, Is.False);
		}
	}
}
