using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Text;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject() : this("Root") {}

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

		private static TextWriter originalOutput;
		private static StringBuilder output;

		[TestFixtureSetUp]
		public static void OneTimeSetUp() {
			originalOutput = Console.Out;
			output = new StringBuilder();
			Console.SetOut(new StringWriter(output));
		}
			
		[SetUp]
		public void SetUp() {
			output.Clear();
		}

		[TestFixtureTearDown]
		public void OneTimeTearDown() {
			Console.SetOut(originalOutput);
		}

		[Test]
		public void TaskName() {
			var task = new Task(() => { });
			var project = new TestProject();
			project.AddTask("foo", task);

			Assert.That(task.Name, Is.EqualTo("foo"));
		}

		[Test]
		public void TaskNameDoesNotExistInRoot() {
			var project = new TestProject();

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInTaskPath() {
			var project = new TestProject();

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInSubProjectInTaskPath() {
			var project = new TestProject();
			new TestProject(project, "testA");

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var project = new TestProject();
			var subProject = new TestProject(project, "testA");
			new TestProject(subProject, "testB");

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var project = new TestProject();
			new TestProject(project, "testA");

			var ex = Assert.Throws<CasperException>(() => project.ExecuteTasks("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void ExecuteTasksInOrder() {
			var project = new TestProject();
			var results = new List<string>();
			project.AddTask("hello", new Task(() => results.Add("hello")));
			project.AddTask("goodbye", new Task(() => results.Add("goodbye")));

			project.ExecuteTasks("goodbye", "hello");

			CollectionAssert.AreEqual(new [] { "goodbye", "hello" }, results);
			Assert.That(output.ToString(), Is.EqualTo("goodbye:\nhello:\n"));
		}

		[Test]
		public void ExecuteTaskWithDependencyGraph() {
			var project = new TestProject();
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
			Assert.That(output.ToString(), Is.EqualTo("wake:\nshower:\ndress:\neat:\nleave:\n"));
		}

		[Test]
		public void ExecuteTasksFromSubProject() {

			var results = new List<string>();

			var project = new TestProject();
			var hello = new Task(() => results.Add("hello"));
			project.AddTask("hello", hello);
			var subProject = new TestProject(project, "testA");
			var goodbye = new Task(() => results.Add("goodbye")) { DependsOn = new [] { hello }};
			subProject.AddTask("goodbye", goodbye);

			project.ExecuteTasks("testA:goodbye");

			CollectionAssert.AreEqual(new [] { "hello", "goodbye" }, results);
			Assert.That(output.ToString(), Is.EqualTo("hello:\ngoodbye:\n"));
		}
	}
}
