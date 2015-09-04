using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Casper {
	[TestFixture]
	public class MSBuildTests {
		[Test]
		public void MSBuild() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent().Parent().Parent().Parent().SubDirectory("Console");

			var outputDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Release");
			if (File.Exists(outputDirectory)) {
				Directory.Delete(outputDirectory, true);
			}

			var msbuild = new MSBuild {
				ProjectFile = consoleProjDir.File("Console.csproj"),
				Targets = new [] { "Build" },
				Properties = new Dictionary<string, string> { { "Configuration", "Release" } },
			};

			msbuild.Execute();

			Assert.True(outputDirectory.File("casper.exe").Exists());
		}

		[Test]
		public void DefaultTargets() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent().Parent().Parent().Parent().SubDirectory("Console");

			var outputDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Debug");
			if (File.Exists(outputDirectory)) {
				Directory.Delete(outputDirectory, true);
			}

			var msbuild = new MSBuild {
				WorkingDirectory = consoleProjDir,
			};

			msbuild.Execute();

			Assert.True(outputDirectory.File("casper.exe").Exists());
		}
	}
}
