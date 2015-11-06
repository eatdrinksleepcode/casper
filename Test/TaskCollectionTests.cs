﻿using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class TaskCollectionTests {

		private class TestProject : ProjectBase {

			public TestProject() : base(null, new DirectoryInfo(Directory.GetCurrentDirectory())) {
			}
			
			public override void Configure() {
			}
		}

		[Test]
		public void MissingTask() {
			var tasks = new TaskCollection(new TestProject());
			var ex = Assert.Throws<CasperException>(() => { var t = tasks["doesNotExist"]; });
			Assert.That(ex.Message, Is.EqualTo("Task 'doesNotExist' does not exist in root project"));
		}
	}
}
