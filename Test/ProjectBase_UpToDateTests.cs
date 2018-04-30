using System;
using System.Collections.Generic;
using Casper.IO;
using NUnit.Framework;

namespace Casper {
	public class ProjectBase_UpToDateTests {

		private IFileSystem fileSystem;
		private IFile testInputFile;
		private IFile testOutputFile;

		[SetUp]
		public void SetUp() {
			fileSystem = new StubFileSystem();
			testInputFile = fileSystem.File("input.txt");
			testInputFile.WriteAllText("Input");
			testOutputFile = fileSystem.File("output.txt");
			testOutputFile.WriteAllText("Output");
		}

		private bool CreateUpToDateTask(IEnumerable<IFile> inputFiles, IEnumerable<IFile> outputFiles) {
			var task = new TestTask();
			foreach(var file in inputFiles) {
				if(null != file) {
					task.AddInput(file);
				}
			}
			foreach(var file in outputFiles) {
				if(null != file) {
					task.AddOutput(file);
				}
			}

			var project = new TestProject(fileSystem);
			project.AddTask("up-to-date", task);
			return project.Execute(project.Tasks["up-to-date"]);
		}

		private bool ExecuteTwice(IFile taskInputFile, IFile taskOutputFile, Action between = null) {
			var inputFiles = new[] { taskInputFile };
			var outputFiles = new[] { taskOutputFile };
			Assert.True(CreateUpToDateTask(inputFiles, outputFiles));

			between?.Invoke();

			return CreateUpToDateTask(inputFiles, outputFiles);
		}

		[Test]
		public void InputsAndOutputsExist() {
			Assert.False(ExecuteTwice(testInputFile, testOutputFile));
		}

		[Test]
		public void InputsAndOutputsDoNotExist() {
			Assert.False(ExecuteTwice(fileSystem.File("missing-input.txt"), fileSystem.File("missing-output.txt")));
		}

		[Test]
		public void NoInput() {
			Assert.False(ExecuteTwice(null, testOutputFile));
		}

		[Test]
		public void NoOutput() {
			Assert.True(ExecuteTwice(testInputFile, null));
		}

		[Test]
		public void InputChanges() {
			Assert.True(ExecuteTwice(testInputFile, testOutputFile, () => { 
				System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();
				testInputFile.WriteAllText("Changed Input");
			}));
		}

		[Test]
		public void OutputChanges() {
			Assert.True(ExecuteTwice(testInputFile, testOutputFile, () => { 
				System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();
				testOutputFile.WriteAllText("Changed Output");
			}));
		}

		[Test]
		public void InputGoesMissing() {
			Assert.True(ExecuteTwice(testInputFile, testOutputFile, () => { testInputFile.Delete(); }));
		}

		[Test]
		public void OutputGoesMissing() {
			Assert.True(ExecuteTwice(testInputFile, testOutputFile, () => { testOutputFile.Delete(); }));
		}

		[Test]
		public void InputAdded() {
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile }));
			Assert.True(CreateUpToDateTask(new[] { testInputFile, fileSystem.File("new-input.txt") }, new[] { testOutputFile }));
		}

		[Test]
		public void OutputAdded() {
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile }));
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile, fileSystem.File("new-output.txt") }));
		}

		[Test]
		public void InputRemoved() {
			Assert.True(CreateUpToDateTask(new[] { testInputFile, fileSystem.File("new-input.txt") }, new[] { testOutputFile }));
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile }));
		}

		[Test]
		public void OutputRemoved() {
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile, fileSystem.File("new-output.txt") }));
			Assert.True(CreateUpToDateTask(new[] { testInputFile }, new[] { testOutputFile }));
		}
	}
}
