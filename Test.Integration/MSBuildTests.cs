using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Casper.IO;

namespace Casper {
	[TestFixture]
	public class MSBuildTests {

		IFileSystem fileSystem = new RealFileSystem();

		[Test]
		public void MSBuild() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent(4).SubDirectory("Core");

			var outputDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Release");
			if (Directory.Exists(outputDirectory)) {
				Directory.Delete(outputDirectory, true);
			}

			var msbuild = new MSBuild {
				ProjectFile = consoleProjDir.File("Core.csproj"),
				Targets = new [] { "Build" },
				Properties = new Dictionary<string, string> { { "Configuration", "Release" } },
			};

			msbuild.Execute(fileSystem);

			Assert.True(outputDirectory.File("Casper.Core.dll").Exists());
		}

		[Test]
		public void DefaultTargets() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent(4).SubDirectory("Core");

			var outputDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Debug");
			if (Directory.Exists(outputDirectory)) {
				Directory.Delete(outputDirectory, true);
			}

			var msbuild = new MSBuild {
				WorkingDirectory = consoleProjDir,
			};

			msbuild.Execute(fileSystem);

			Assert.True(outputDirectory.File("Casper.Core.dll").Exists());
		}
	}
}
