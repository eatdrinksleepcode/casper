using NUnit.Framework;

namespace Casper.IO {
	[TestFixture]
	public class StubDirectoryTests : DirectoryTests {
		protected override IFileSystem GetFileSystemInstance() {
			return new StubFileSystem();
		}
	}
}
