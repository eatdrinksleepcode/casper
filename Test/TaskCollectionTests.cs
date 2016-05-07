using NUnit.Framework;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class TaskCollectionTests {

		[Test]
		public void MissingTask() {
			var tasks = new TaskCollection(new TestProject(new StubFileSystem()));
			var ex = Assert.Throws<UnknownTaskException>(() => { var t = tasks["doesNotExist"]; });
			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}
	}
}
