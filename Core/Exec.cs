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
				throw new CasperException(CasperException.KnownExitCode.ConfigurationError, "Must set 'Executable'");
			}
			Process process;
			StringBuilder allOutput = new StringBuilder();
			if(Environment.IsUnix) {
				var processStartInfo = new ProcessStartInfo {
					FileName = Executable,
					Arguments = Arguments,
					WorkingDirectory = WorkingDirectory,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};
				process = StartProcessAndWatch(allOutput, processStartInfo);
			} else {
				var processStartInfo = new ProcessStartInfo {
					FileName = "cmd.exe",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
				};
				process = StartProcessAndWatch(allOutput, processStartInfo);
				if(null != WorkingDirectory) {
					process.StandardInput.WriteLine("cd \"{0}\"", WorkingDirectory);
				}
				process.StandardInput.WriteLine("{0} {1}", Executable, Arguments);
				process.StandardInput.WriteLine("exit %errorlevel%");
				process.StandardInput.Flush();
				process.StandardInput.Close();
			}
			process.WaitForExit();
			if (0 != process.ExitCode) {
				Console.Error.WriteLine(allOutput.ToString());
				throw new CasperException(CasperException.KnownExitCode.TaskFailed, "Process '{0}{1}'{2} exited with code {3}", Executable, null == Arguments ? "" : " " + Arguments, null == WorkingDirectory ? "" : " in '" + WorkingDirectory + "'", process.ExitCode);
			}
		}

		static Process StartProcessAndWatch(StringBuilder allOutput, ProcessStartInfo processStartInfo) {
			Process process = Process.Start(processStartInfo);
			process.ErrorDataReceived += (sender, e) => { if(e.Data != null) allOutput.AppendLine(e.Data); };
			process.OutputDataReceived += (sender, e) => { if(e.Data != null) allOutput.AppendLine(e.Data); };
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return process;
		}
	}
}
