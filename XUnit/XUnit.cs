using System;
using System.Collections.Generic;
using Casper.IO;
using Xunit.Runners;
using System.Threading;

namespace Casper {
	public class XUnit : TaskBase {

		public string TestAssembly { get; set; }
		public string TestName { get; set; }

		public override void Execute(IFileSystem fileSystem) {
			if(null == TestAssembly) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'TestAssembly'");
			}

			var runner = AssemblyRunner.WithoutAppDomain(TestAssembly);
			if(null != TestName) {
				runner.TestCaseFilter = (arg) => {
					return arg.DisplayName == TestName;
				};
			}
			var errors = new List<TestError>();
			int started = 0, finished = 0;
			runner.OnTestStarting += _ => started++;
			runner.OnTestFinished += _ => finished++;
			runner.OnTestFailed += (TestFailedInfo info) => {
				errors.Add(new TestError { Name = info.TestDisplayName, Message = info.ExceptionMessage, StackTrace = info.ExceptionStackTrace });
			};
			var wait = new ManualResetEvent(false);
			runner.OnExecutionComplete += (obj) => wait.Set();
			runner.Start();
			wait.WaitOne();
			if(started == 0 || finished == 0) {
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "No tests executed");
			}
			HandleFailures(errors);
		}

		private void HandleFailures(List<TestError> failures) {
			if(failures.Count > 0) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("Failing tests:");
				Console.Error.WriteLine();
				foreach(var error in failures) {
					Console.Error.WriteLine("{0}:", error.Name);
					Console.Error.WriteLine(error.Message);
					Console.Error.WriteLine(error.StackTrace);
				}
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "{0} tests failed", failures.Count);
			}
		}

		private class TestError {
			public string Name;
			public string Message;
			public string StackTrace;
		}
	}
}

