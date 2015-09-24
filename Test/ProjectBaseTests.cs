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
			var currentDirectory = Directory.GetCurrentDirectory();
			var projectDirectory = currentDirectory.SubDirectory("testA");
			Directory.CreateDirectory(projectDirectory);
			string executeDirectory = null;
			var task = new Task(() => { executeDirectory = Directory.GetCurrentDirectory(); throw new Exception(); });
			var project = new TestProject(projectDirectory);

			Assert.Throws<Exception>(() => project.Execute(task));

			Assert.That(executeDirectory, Is.EqualTo(projectDirectory));
			Assert.That(Directory.GetCurrentDirectory(), Is.EqualTo(currentDirectory));
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
			var currentDirectory = Directory.GetCurrentDirectory();
			var projectDirectory = currentDirectory.SubDirectory("testA");
			var project = new TestProject(currentDirectory);
			new TestProject(project, projectDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var currentDirectory = Directory.GetCurrentDirectory();
			var projectDirectory = currentDirectory.SubDirectory("testA");
			var subProjectDirectory = projectDirectory.SubDirectory("testB");
			var project = new TestProject(currentDirectory);
			var subProject = new TestProject(project, projectDirectory);
			new TestProject(subProject, subProjectDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var currentDirectory = Directory.GetCurrentDirectory();
			var projectDirectory = currentDirectory.SubDirectory("testA");
			var project = new TestProject(currentDirectory);
			new TestProject(project, projectDirectory);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}
	}
}
