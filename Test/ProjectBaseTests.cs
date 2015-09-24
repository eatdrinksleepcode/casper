using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject(string location) : base(null, location) {
			}

			public TestProject(ProjectBase parent, string location) : base(parent, location) {
			}

			public override void Configure() {
			}

			public void ExecuteTasks(params string[] taskNamesToExecute) {
				this.ExecuteTasks((IEnumerable<string>)taskNamesToExecute);
			}

			public new void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
				base.ExecuteTasks(taskNamesToExecute);
			}
		}

		[Test]
		public void TaskName() {
			var task = new Task(() => { });
			var project = new TestProject(null);
			project.AddTask("foo", task);

			Assert.That(task.Name, Is.EqualTo("foo"));
		}

		[Test]
		public void ExecuteTaskRelativeToProjectDirectory() {
			var rootDirectory = Directory.GetCurrentDirectory();
			var firstSubDirectory = rootDirectory.SubDirectory("testA");
			Directory.CreateDirectory(firstSubDirectory);
			string executeDirectory = null;
			var task = new Task(() => { executeDirectory = Directory.GetCurrentDirectory(); throw new Exception(); });
			var project = new TestProject(firstSubDirectory);

			Assert.Throws<Exception>(() => project.Execute(task));

			Assert.That(executeDirectory, Is.EqualTo(firstSubDirectory));
			Assert.That(Directory.GetCurrentDirectory(), Is.EqualTo(rootDirectory));
		}

		[Test]
		public void TaskNameDoesNotExistInRoot() {
			var project = new TestProject(Directory.GetCurrentDirectory());

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInTaskPath() {
			var project = new TestProject(Directory.GetCurrentDirectory());

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInSubProjectInTaskPath() {
			var projectDirectory = Directory.GetCurrentDirectory();
			var firstSubDirectory = projectDirectory.SubDirectory("testA");
			var project = new TestProject(projectDirectory);
			new TestProject(project, firstSubDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var projectDirectory = Directory.GetCurrentDirectory();
			var firstSubDirectory = projectDirectory.SubDirectory("testA");
			var secondSubDirectory = firstSubDirectory.SubDirectory("testB");
			var project = new TestProject(projectDirectory);
			var subProject = new TestProject(project, firstSubDirectory);
			new TestProject(subProject, secondSubDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var projectDirectory = Directory.GetCurrentDirectory();
			var firstSubDirectory = projectDirectory.SubDirectory("testA");
			var project = new TestProject(projectDirectory);
			new TestProject(project, firstSubDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}
	}
}
