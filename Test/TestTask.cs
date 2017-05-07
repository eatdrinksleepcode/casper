using System.Collections.Generic;
using Casper.IO;

namespace Casper {
	public class TestTask : TaskBase {
		private readonly List<IFile> inputFiles = new List<IFile>();
		private readonly List<IFile> outputFiles = new List<IFile>();

		public TestTask(params TaskBase[] dependencies) {
			DependsOn = dependencies;
		}

		public void AddInput(IFile inputFile) {
			inputFiles.Add(inputFile);
		}

		public void AddOutput(IFile inputFile) {
			outputFiles.Add(inputFile);
		}

		public override void Execute(IFileSystem fileSystem) {
		}

		public override IEnumerable<IFile> InputFiles {
			get { return inputFiles; }
		}

		public override IEnumerable<IFile> OutputFiles {
			get { return outputFiles; }
		}
	}
}
