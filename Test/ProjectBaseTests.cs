using System.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject(string name) : base(null, new DirectoryInfo(Directory.GetCurrentDirectory()), name) {
			}

			public TestProject(ProjectBase parent, string name) : base(parent, new DirectoryInfo(Directory.GetCurrentDirectory()), name) {
			}

			public override void Configure() {
			}

			public void ExecuteTasks(params string[] taskNamesToExecute) {
				base.ExecuteTasks(taskNamesToExecute);
			}
		}

		string rootProjectName;
		string firstSubProjectName;
		string secondSubProjectName;

		[SetUp]
		public void SetUp() {
			rootProjectName = "Root";
			firstSubProjectName = "testA";
			secondSubProjectName = "testB";
		}

		[Test]
		public void TaskName() {
			var task = new Task(() => { });
			var project = new TestProject(rootProjectName);
			project.AddTask("foo", task);

			Assert.That(task.Name, Is.EqualTo("foo"));
		}

		[Test]
		public void TaskNameDoesNotExistInRoot() {
			var project = new TestProject(rootProjectName);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInTaskPath() {
			var project = new TestProject(rootProjectName);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInSubProjectInTaskPath() {
			var project = new TestProject(rootProjectName);
			new TestProject(project, firstSubProjectName);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var project = new TestProject(rootProjectName);
			var subProject = new TestProject(project, firstSubProjectName);
			new TestProject(subProject, secondSubProjectName);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var project = new TestProject(rootProjectName);
			new TestProject(project, firstSubProjectName);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}
	}
}
