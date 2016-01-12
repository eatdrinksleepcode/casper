using System.Diagnostics;
using System.Text;
using System;
using Casper.IO;

namespace Casper {
	public class Exec : TaskBase {
		public string WorkingDirectory { get; set; }
		public string Executable { get; set; }
		public string Arguments { get; set; }

		public override void Execute(IFileSystem fileSystem) {
			if (null == Executable) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Source'");
			}
			var processStartInfo = new ProcessStartInfo {
				FileName = Executable,
				Arguments = Arguments,
				UseShellExecute = false,
				WorkingDirectory = WorkingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			var process = Process.Start(processStartInfo);
			var allOutput = new StringBuilder();
			process.ErrorDataReceived += (sender, e) => allOutput.AppendLine(e.Data);
			process.OutputDataReceived += (sender, e) => allOutput.AppendLine(e.Data);
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			if (0 != process.ExitCode) {
				Console.Error.WriteLine(allOutput.ToString());
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "Process '{0}{1}' exited with code {2}", Executable, null == Arguments ? "" : " " + Arguments, process.ExitCode);
			}
		}
	}
}
