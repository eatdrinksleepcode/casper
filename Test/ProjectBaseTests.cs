using System;
using System.Collections.Generic;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

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

			var ex = Assert.Throws<UnknownTaskException>(() => project.BuildTaskExecutionGraph("doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInTaskPath() {
			var project = new TestProject(fileSystem);

			var ex = Assert.Throws<CasperException>(() => project.BuildTaskExecutionGraph("doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in root project"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInSubProjectInTaskPath() {
			var project = new TestProject(fileSystem);
			new TestProject(project, "testA");

			var ex = Assert.Throws<CasperException>(() => project.BuildTaskExecutionGraph("testA:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA'"));
		}

		[Test]
		public void SubProjectNameDoesNotExistInMultiSubProjectInTaskPath() {
			var project = new TestProject(fileSystem);
			var subProject = new TestProject(project, "testA");
			new TestProject(subProject, "testB");

			var ex = Assert.Throws<CasperException>(() => project.BuildTaskExecutionGraph("testA:testB:doesNotExist:foo"));

			Assert.That(ex.Message, Is.EqualTo("Project 'doesNotExist' does not exist in project 'testA:testB'"));
		}

		[Test]
		public void TaskNameDoesNotExistInSubProject() {
			var project = new TestProject(fileSystem);
			new TestProject(project, "testA");

			var ex = Assert.Throws<UnknownTaskException>(() => project.BuildTaskExecutionGraph("testA:doesNotExist"));

			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in project 'testA'"));
		}

		// TODO: these should probably either test ProjectBase.BuildTaskExecutionGraph or TaskExecutionGraph.ExecuteTasks

		[Test]
		public void ExecuteTasksInOrder() {
			var project = new TestProject(fileSystem);
			var results = new List<string>();
			project.AddTask("hello", new Task(() => results.Add("hello")));
			project.AddTask("goodbye", new Task(() => results.Add("goodbye")));

			project.ExecuteTasks("goodbye", "hello");

			CollectionAssert.AreEqual(new [] { "goodbye", "hello" }, results);
			Assert.That(output.ToString(), Is.EqualTo("goodbye\nhello\n".NormalizeNewLines()));
		}

		[Test]
		public void ExecuteTaskWithComplexDependencyGraph() {
			var rootProject = new TestProject(fileSystem);

			var coreProject = new TestProject(rootProject, "Core");
			var coreCompile = coreProject.AddTestTask("Compile");

			var nunitProject = new TestProject(rootProject, "NUnit");
			var nunitCompile = nunitProject.AddTestTask("Compile", coreCompile);

			var msbuildProject = new TestProject(rootProject, "MSBuild");
			var msbuildCompile = msbuildProject.AddTestTask("Compile", coreCompile);

			var csharpProject = new TestProject(rootProject, "CSharp");
			var csharpCompile = csharpProject.AddTestTask("Compile", coreCompile);

			var consoleProject = new TestProject(rootProject, "Console");
			var consoleCompile = consoleProject.AddTestTask("Compile", coreCompile, msbuildCompile, nunitCompile);
			consoleProject.AddTestTask("Pack", consoleCompile);

			var testProject = new TestProject(rootProject, "Test");
			testProject.AddTestTask("Compile", consoleCompile);

			var integrationProject = new TestProject(rootProject, "Test.Integration");
			integrationProject.AddTestTask("Compile", consoleCompile, csharpCompile);

			rootProject.ExecuteTasks(new[] { "Console:Pack", "Test:Compile", "Test.Integration:Compile" });

			var result = output.ToString();
			StringAssert.Contains("CSharp:Compile", result);
			StringAssert.Contains("Test.Integration:Compile", result);
			Assert.That(result.IndexOf("CSharp:Compile"), Is.LessThan(result.IndexOf("Test.Integration:Compile")));
		}

		[Test]
		public void ExecuteTasksFromSubProject() {

			var results = new List<string>();

			var project = new TestProject(fileSystem);
			var hello = new Task(() => results.Add("hello"));
			project.AddTask("hello", hello);
			var subProject = new TestProject(project, "testA");
			var goodbye = new Task(() => results.Add("goodbye")) { DependsOn = new [] { hello }};
			subProject.AddTask("goodbye", goodbye);

			project.ExecuteTasks("testA:goodbye");

			CollectionAssert.AreEqual(new [] { "hello", "goodbye" }, results);
			Assert.That(output.ToString(), Is.EqualTo("hello\ntestA:goodbye\n".NormalizeNewLines()));
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
