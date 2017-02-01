using NUnit.Framework;

namespace Casper.IO {
	[TestFixture]
	public class RealDirectoryTests : DirectoryTests {
		protected override IFileSystem GetFileSystemInstance() {
			return RealFileSystem.Instance;
		}
	}
}
