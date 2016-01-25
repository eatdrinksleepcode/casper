using System;
using System.Collections.Generic;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	public class ProjectBase_UpToDateTests {

		private IFileSystem fileSystem;
		private IFile inputFile;
		private IFile outputFile;

		[SetUp]
		public void SetUp() {
			fileSystem = new StubFileSystem();
			inputFile = fileSystem.File("input.txt");
			inputFile.WriteAllText("Input");
			outputFile = fileSystem.File("output.txt");
			outputFile.WriteAllText("Output");
		}

		private class TestProject : ProjectBase {

			public TestProject(IFileSystem fileSystem) : base(null, "test", fileSystem, "Root") {}

			public override void Configure() {
			}

			public void ExecuteTasks(params string[] taskNamesToExecute) {
				base.ExecuteTasks(taskNamesToExecute);
			}
		}

		private class TestTask : TaskBase {
			private readonly List<IFile> inputFiles = new List<IFile>();
			private readonly List<IFile> outputFiles = new List<IFile>();

			public void AddInput(IFile inputFile) {
				inputFiles.Add(inputFile);
			}

			public void AddOutput(IFile inputFile) {
				outputFiles.Add(inputFile);
			}

			public override void Execute(IFileSystem fileSystem) {
			}

			protected override IEnumerable<IFile> InputFiles {
				get {
					return inputFiles;
				}
			}

			protected override IEnumerable<IFile> OutputFiles {
				get {
					return outputFiles;
				}
			}
		}

		private bool CreateUpToDateTask(IFile[] inputFiles, IFile[] outputFiles) {
			var task = new TestTask();
			foreach(var inputFile in inputFiles) {
				if(null != inputFile) {
					task.AddInput(inputFile);
				}
			}
			foreach(var outputFile in outputFiles) {
				if(null != outputFile) {
					task.AddOutput(outputFile);
				}
			}

			var project = new TestProject(fileSystem);
			project.AddTask("up-to-date", task);
			return project.Execute(project.Tasks["up-to-date"]);
		}

		private bool ExecuteTwice(IFile inputFile, IFile outputFile, Action between = null) {
			var inputFiles = new[] { inputFile };
			var outputFiles = new[] { outputFile };
			Assert.True(CreateUpToDateTask(inputFiles, outputFiles));

			if(null != between) {
				between();
			}

			return CreateUpToDateTask(inputFiles, outputFiles);
		}

		[Test]
		public void InputsAndOutputsExist() {
			Assert.False(ExecuteTwice(inputFile, outputFile));
		}

		[Test]
		public void InputsAndOutputsDoNotExist() {
			Assert.False(ExecuteTwice(fileSystem.File("missing-input.txt"), fileSystem.File("missing-output.txt")));
		}

		[Test]
		public void NoInput() {
			Assert.False(ExecuteTwice(null, outputFile));
		}

		[Test]
		public void NoOutput() {
			Assert.True(ExecuteTwice(inputFile, null));
		}

		[Test]
		public void InputChanges() {
			Assert.True(ExecuteTwice(inputFile, outputFile, () => { inputFile.WriteAllText("Changed Input"); }));
		}

		[Test]
		public void OutputChanges() {
			Assert.True(ExecuteTwice(inputFile, outputFile, () => { outputFile.WriteAllText("Changed Output"); }));
		}

		[Test]
		public void InputGoesMissing() {
			Assert.True(ExecuteTwice(inputFile, outputFile, () => { inputFile.Delete(); }));
		}

		[Test]
		public void OutputGoesMissing() {
			Assert.True(ExecuteTwice(inputFile, outputFile, () => { outputFile.Delete(); }));
		}

		[Test]
		public void InputAdded() {
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile }));
			Assert.True(CreateUpToDateTask(new[] { inputFile, fileSystem.File("new-input.txt") }, new[] { outputFile }));
		}

		[Test]
		public void OutputAdded() {
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile }));
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile, fileSystem.File("new-output.txt") }));
		}

		[Test]
		public void InputRemoved() {
			Assert.True(CreateUpToDateTask(new[] { inputFile, fileSystem.File("new-input.txt") }, new[] { outputFile }));
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile }));
		}

		[Test]
		public void OutputRemoved() {
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile, fileSystem.File("new-output.txt") }));
			Assert.True(CreateUpToDateTask(new[] { inputFile }, new[] { outputFile }));
		}
	}
}
