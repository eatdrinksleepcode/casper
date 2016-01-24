using System;
using System.IO;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	[TestFixture]
	public class ProjectBaseTests {

		private class TestProject : ProjectBase {

			public TestProject(string location) : base(null, location, new RealFileSystem()) {
			}

			public override void Configure() {
			}
		}

		[Test]
		public void ExecuteTaskRelativeToProjectDirectory() {
			DirectoryInfo rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
			DirectoryInfo firstSubDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "testA"));
			firstSubDirectory.Create();

			string executeDirectory = null;
			var task = new Task(() => { executeDirectory = Directory.GetCurrentDirectory(); throw new Exception(); });
			var project = new TestProject(firstSubDirectory.FullName);
			project.AddTask("test", task);

			Assert.Throws<Exception>(() => project.Execute(task));

			Assert.That(executeDirectory, Is.EqualTo(firstSubDirectory.FullName));
			Assert.That(Directory.GetCurrentDirectory(), Is.EqualTo(rootDirectory.FullName));
		}
	}
}

