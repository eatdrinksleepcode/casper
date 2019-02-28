using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class CscTests {

		IFileSystem fileSystem = RealFileSystem.Instance;
		IDirectory outputDir;

		[SetUp]
		public void SetUp() {
			outputDir = fileSystem.MakeTemporaryDirectory();
		}

		[TearDown]
		public void TearDown() {
			outputDir.Delete();
		}

		[Test]
		public void Csc() {
			var projectDirectory = fileSystem.File(typeof(TaskBase).Assembly.Location).Directory.Parent.Parent.Parent.Directory("Core");

			var outputDirectory = projectDirectory.Directory("bin/Release");
			outputDirectory.Delete();

            var outputFile = outputDir.File("CscTest.dll");

			var csc = new Csc {
				WorkingDirectory = projectDirectory,
				SystemReferences = new [] { "mscorlib", "System", "System.Core", },
				OutputFileName = outputFile,
			};

			csc.Execute(fileSystem);

			Assert.True(outputFile.Exists());
		}
	}
}
