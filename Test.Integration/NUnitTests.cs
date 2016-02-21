using NUnit.Framework;
using System.Reflection;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class NUnitTests {

		private RedirectedStandardOutput error;
		IFileSystem fileSystem = RealFileSystem.Instance;

		[SetUp]
		public void SetUp() {
			error = RedirectedStandardOutput.RedirectError();
		}

		[TearDown]
		public void TearDown() {
			error.Dispose();
		}
		
		[Test]
		public void Pass() {
			var task = new NUnit {
				TestAssembly = Assembly.GetExecutingAssembly().Location,
				TestName = "Casper.NUnitTests.ShouldPass",
			};

			Assert.DoesNotThrow(() => task.Execute(fileSystem));
		}

		[Test]
		public void Fail() {
			var task = new NUnit {
				TestAssembly = Assembly.GetExecutingAssembly().Location,
				TestName = "Casper.NUnitTests.ShouldFail",
			};

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));

			Assert.That(error.ToString(), Does.StartWith(@"
Failing tests:

Casper.NUnitTests.ShouldFail:
Failed!
at Casper.NUnitTests.ShouldFail()".NormalizeNewLines()));
		}

		[Test]
		public void MissingTestAssembly() {
			var task = new NUnit();

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));
		}

		[Test, Explicit]
		public void ShouldPass() {
			Assert.Pass();
		}

		[Test, Explicit]
		public void ShouldFail() {
			Assert.Fail("Failed!");
		}
	}
}
