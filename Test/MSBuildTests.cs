using NUnit.Framework;
using System.IO;

namespace Casper {
	[TestFixture]
	public class MSBuildTests {
		[Test]
		public void MSBuild() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent().Parent().Parent().Parent().SubDirectory("Console");

			var debugDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Debug");
			if (File.Exists(debugDirectory)) {
				Directory.Delete(debugDirectory, true);
			}

			var msbuild = new MSBuild {
				ProjectFile = consoleProjDir.File("Console.csproj"),
				Targets = new [] { "Build" },
			};

			msbuild.Execute();

			Assert.True(debugDirectory.File("casper.exe").Exists());
		}

		[Test]
		public void DefaultTargets() {
			var consoleProjDir = typeof(TaskBase).Assembly.Location.Parent().Parent().Parent().Parent().SubDirectory("Console");

			var debugDirectory = consoleProjDir.SubDirectory("bin").SubDirectory("Debug");
			if (File.Exists(debugDirectory)) {
				Directory.Delete(debugDirectory, true);
			}

			var msbuild = new MSBuild {
				WorkingDirectory = consoleProjDir,
			};

			msbuild.Execute();

			Assert.True(debugDirectory.File("casper.exe").Exists());
		}
	}
}
