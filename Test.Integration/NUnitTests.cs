using NUnit.Framework;
using System.Reflection;

namespace Casper {
	[TestFixture]
	public class NUnitTests {

		private RedirectedStandardOutput error;

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

			Assert.DoesNotThrow(() => task.Execute());
		}

		[Test]
		public void Fail() {
			var task = new NUnit {
				TestAssembly = Assembly.GetExecutingAssembly().Location,
				TestName = "Casper.NUnitTests.ShouldFail",
			};

			Assert.Throws<CasperException>(() => task.Execute());

			Assert.That(error.ToString(), Is.EqualTo(@"
Failing tests:

Casper.NUnitTests.ShouldFail: Failed!
"));
		}

		[Test]
		public void MissingTestAssembly() {
			var task = new NUnit();

			Assert.Throws<CasperException>(() => task.Execute());
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
