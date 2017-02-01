using NUnit.Framework;

namespace Casper.IO {
	[TestFixture]
	public class RealFileTests : FileTests {
		protected override IFileSystem GetFileSystemInstance() {
			return RealFileSystem.Instance;
		}
	}
}
