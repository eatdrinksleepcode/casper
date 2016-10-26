using System.Reflection;
using Casper.IO;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Casper {
	[TestFixture]
	public class XUnitTests {

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
			var task = new XUnit {
				TestAssembly = Assembly.GetExecutingAssembly().Location,
				TestName = "Casper.XUnitTests.ShouldPass",
			};

			Assert.DoesNotThrow(() => task.Execute(fileSystem));
		}

		[Test]
		public void Fail() {
			var task = new XUnit {
				TestAssembly = Assembly.GetExecutingAssembly().Location,
				TestName = "Casper.XUnitTests.ShouldFail",
			};

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));

			Assert.That(error.ToString(), Does.StartWith(@"
Failing tests:

Casper.XUnitTests.ShouldFail:
Failed!
  at Casper.XUnitTests.ShouldFail".NormalizeNewLines()));
		}

		[Test]
		public void MissingTestAssembly() {
			var task = new XUnit();

			Assert.Throws<CasperException>(() => task.Execute(fileSystem));
		}

		[Fact]
		public void ShouldPass() {
			// Pass
		}

		[Fact]
		public void ShouldFail() {
			throw new System.Exception("Failed!");
		}
	}
}
