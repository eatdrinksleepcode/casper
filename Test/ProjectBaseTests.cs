using NUnit.Framework;


namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject() : base(null) {
			}
			
			public override void Configure() {
			}
		}
		
		[Test]
		public void TaskName() {
			var task = new Task(() => { });
			var project = new TestProject();
			project.AddTask("foo", task);

			Assert.That(task.Name, Is.EqualTo("foo"));
		}
	}
}
