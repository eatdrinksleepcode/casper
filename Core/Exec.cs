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
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Executable'");
			}
			var processStartInfo = new ProcessStartInfo {
				FileName = "cmd.exe",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
                RedirectStandardInput = true,
			};
			var process = Process.Start(processStartInfo);
			var allOutput = new StringBuilder();
			process.ErrorDataReceived += (sender, e) => { if(e.Data != null) allOutput.AppendLine(e.Data); };
			process.OutputDataReceived += (sender, e) => { if(e.Data != null) allOutput.AppendLine(e.Data); };
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
            if(null != WorkingDirectory)
            {
                process.StandardInput.WriteLine("cd \"{0}\"", WorkingDirectory);
            }
            process.StandardInput.WriteLine("{0} {1}", Executable, Arguments);
            process.StandardInput.WriteLine("exit %errorlevel%");
            process.StandardInput.Flush();
            process.StandardInput.Close();
			process.WaitForExit();
			if (0 != process.ExitCode) {
				Console.Error.WriteLine(allOutput.ToString());
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "Process '{0}{1}'{2} exited with code {3}", Executable, null == Arguments ? "" : " " + Arguments, null == WorkingDirectory ? "" : " in '" + WorkingDirectory + "'", process.ExitCode);
			}
		}
	}
}
