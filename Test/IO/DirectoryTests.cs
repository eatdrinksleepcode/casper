using System.Reflection;
using NUnit.Framework;

namespace Casper.IO {
	public abstract class DirectoryTests {
		private IFileSystem fileSystem;
		private IDirectory testParentDirectory;

		[TestFixtureSetUp]
		public void SetUpOnce() {
			fileSystem = GetFileSystemInstance();
			testParentDirectory = fileSystem.File(Assembly.GetExecutingAssembly().CodeBase).Directory;
		}

		protected abstract IFileSystem GetFileSystemInstance();

		[Test]
		public void RootDirectory() {
			if(Environment.IsUnixFileSystem) {
				Assert.That(testParentDirectory.RootDirectory.FullPath, Is.EqualTo("/"));
			} else {
				Assert.Inconclusive("Can't predetermine root path on Windows");
			}
		}

		[Test]
		public void CreateDelete() {
			var testDirectory = testParentDirectory.Directory(typeof(FileTests).Name);
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
