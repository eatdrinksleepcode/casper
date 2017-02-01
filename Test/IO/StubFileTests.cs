using NUnit.Framework;

namespace Casper.IO {
	[TestFixture]
	public class StubFileTests : FileTests {
		protected override IFileSystem GetFileSystemInstance() {
			return new StubFileSystem();
		}
	}
}
