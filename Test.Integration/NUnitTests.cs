using NUnit.Framework;
using System.Reflection;

namespace Casper {
	[TestFixture]
	public class NUnitTests {
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
			Assert.Fail();
		}
	}
}
