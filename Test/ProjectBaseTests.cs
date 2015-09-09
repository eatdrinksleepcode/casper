using NUnit.Framework;
using System.IO;
using System;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject(string location) : base(null, location) {
			}
			
			public override void Configure() {
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
	}
}
