using System;
using System.Collections.Generic;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject(IFileSystem fileSystem) : this("Root", fileSystem) {}

			public TestProject(IFileSystem fileSystem, IDirectory location) : base(null, location.Path, fileSystem) {
			}

			public TestProject(string name, IFileSystem fileSystem) : base(null, "test", fileSystem, name) {
			}

			public TestProject(ProjectBase parent, string name, IFileSystem fileSystem) : base(parent, "test", fileSystem, name) {
			}

			public override void Configure() {
			}

			public void ExecuteTasks(params string[] taskNamesToExecute) {
				base.ExecuteTasks(taskNamesToExecute);
			}
		}

		private static RedirectedStandardOutput output;

		private IFileSystem fileSystem;

		[TestFixtureSetUp]
		public static void OneTimeSetUp() {
			output = RedirectedStandardOutput.RedirectOut();
		}
			
		[SetUp]
		public void SetUp() {
			output.Clear();
			fileSystem = new StubFileSystem();
		}

		[TestFixtureTearDown]
		public void OneTimeTearDown() {
			output.Dispose();
		}

		[Test]
		public void TaskName() {
			var task = new Task(() => { });
			var project = new TestProject(fileSystem);
			project.AddTask("foo", task);

			Assert.That(task.Name, Is.EqualTo("foo"));
		}

		[Test]
		public void TaskNameDoesNotExistInRoot() {
			var project = new TestProject(fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInTaskPath() {
			var project = new TestProject(fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInSubProjectInTaskPath() {
			var project = new TestProject(fileSystem);
			new TestProject(project, "testA", fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var project = new TestProject(fileSystem);
			var subProject = new TestProject(project, "testA", fileSystem);
			new TestProject(subProject, "testB", fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var project = new TestProject(fileSystem);
			new TestProject(project, "testA", fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void ExecuteTasksInOrder() {
			var project = new TestProject(fileSystem);
			var results = new List<string>();
			project.AddTask("hello", new Task(() => results.Add("hello")));
			project.AddTask("goodbye", new Task(() => results.Add("goodbye")));

			project.ExecuteTasks("goodbye", "hello");

			CollectionAssert.AreEqual(new [] { "goodbye", "hello" }, results);
			Assert.That(output.ToString(), Is.EqualTo("goodbye\nhello\n"));
		}

		[Test]
		public void ExecuteTaskWithDependencyGraph() {
			var project = new TestProject(fileSystem);
			var results = new List<string>();
			var wake = new Task(() => results.Add("wake"));
			project.AddTask("wake", wake);
			var shower = new Task(() => results.Add("shower")) { DependsOn = new[] { wake	} };
			project.AddTask("shower", shower);
			var eat = new Task(() => results.Add("eat")) { DependsOn = new[] { wake	} };
			project.AddTask("eat", eat);
			var dress = new Task(() => results.Add("dress")) { DependsOn = new[] { shower }	};
			project.AddTask("dress", dress);
			project.AddTask("sleep", new Task(() => results.Add("sleep")));
			var leave = new Task(() => results.Add("leave")) { DependsOn = new[] { dress, eat } };
			project.AddTask("leave", leave);

			project.ExecuteTasks("leave");

			CollectionAssert.AreEqual(new [] { "wake", "shower", "dress", "eat", "leave" }, results);
			Assert.That(output.ToString(), Is.EqualTo("wake\nshower\ndress\neat\nleave\n"));
		}

		[Test]
		public void ExecuteTasksFromSubProject() {

			var results = new List<string>();

			var project = new TestProject(fileSystem);
			var hello = new Task(() => results.Add("hello"));
			project.AddTask("hello", hello);
			var subProject = new TestProject(project, "testA", fileSystem);
			var goodbye = new Task(() => results.Add("goodbye")) { DependsOn = new [] { hello }};
			subProject.AddTask("goodbye", goodbye);

			project.ExecuteTasks("testA:goodbye");

			CollectionAssert.AreEqual(new [] { "hello", "goodbye" }, results);
			Assert.That(output.ToString(), Is.EqualTo("hello\ntestA:goodbye\n"));
		}

		[Test]
		public void ExecuteTaskRelativeToProjectDirectory() {
			IDirectory rootDirectory = fileSystem.GetCurrentDirectory();
			IDirectory firstSubDirectory = rootDirectory.Directory("testA");;

			IDirectory executeDirectory = null;
			var task = new Task(() => { executeDirectory = fileSystem.GetCurrentDirectory(); throw new Exception(); });
			var project = new TestProject(fileSystem, firstSubDirectory);
			project.AddTask("test", task);

			Assert.Throws<Exception>(() => project.Execute(task));

			Assert.That(executeDirectory, Is.EqualTo(firstSubDirectory));
			Assert.That(fileSystem.GetCurrentDirectory(), Is.EqualTo(rootDirectory));
		}

		[Test]
		[Ignore("Need to detect cyclical dependencies")]
		public void DetectCyclicalDependencies() {
			var project = new TestProject(fileSystem);
			var a = new Task(() => { });
			var b = new Task(() => { }) { DependsOn = new[] { a } };
			a.DependsOn = new[] { b };
			project.AddTask("a", a);
			project.AddTask("b", b);
			Assert.Throws<CasperException>(() => project.ExecuteTasks("a"));
		}
	}
}
