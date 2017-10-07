using System.Reflection;
using NUnit.Framework;
using static System.IO.Path;

namespace Casper.IO {
	public abstract class FileTests {

		private IFileSystem fileSystem;
		private IDirectory testParentDirectory;
		private IDirectory testDirectory;
		private IDirectory originalWorkingDirectory;

		[TestFixtureSetUp]
		public void SetUpOnce() {
			fileSystem = GetFileSystemInstance();
			testParentDirectory = fileSystem.File(Assembly.GetExecutingAssembly().CodeBase).Directory;
			testDirectory = testParentDirectory.Directory(typeof(FileTests).Name);
			if(testDirectory.Exists()) {
				testDirectory.Delete();
			}
			testDirectory.Create();
		}

		protected abstract IFileSystem GetFileSystemInstance();

		[TestFixtureTearDown]
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
			var fileName = "foo.txt";
			var file = fileSystem.File(fileName);
			Assert.That(file.FullPath, Is.EqualTo(Combine(testParentDirectory.FullPath, typeof(FileTests).Name, fileName)));
			Assert.That(file.Directory.FullPath, Is.EqualTo(Combine(testParentDirectory.FullPath, typeof(FileTests).Name)));
		}

		[Test]
		public void AbsoluteFile() {
			var rootPath = testDirectory.RootDirectory.FullPath;
			var fileName = Combine(rootPath, "foo.txt");
			var file = fileSystem.File(fileName);
			Assert.That(file.FullPath, Is.EqualTo(Combine(rootPath, fileName)));
			Assert.That(file.Directory.FullPath, Is.EqualTo(rootPath));
		}

		[Test]
		public void File() {
			var fileName = "foo.txt";
			var file = fileSystem.File(fileName);

			Assert.That(file.Exists, Is.False);

			file.WriteAllText("foo");

			Assert.That(file.Exists, Is.True);

			file.Delete();

			Assert.That(file.Exists, Is.False);
		}
	}
}
