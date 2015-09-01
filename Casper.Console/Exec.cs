using System.Diagnostics;

namespace Casper {
	public class Exec : TaskBase {
		public string Executable { get; set; }
		public string Arguments { get; set; }

		public override void Execute() {
			if (null == Executable) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Source'");
			}
			var processStartInfo = new ProcessStartInfo {
				FileName = Executable,
				Arguments = Arguments,
				UseShellExecute = false,
			};
			var process = Process.Start(processStartInfo);
			process.WaitForExit();
			if (0 != process.ExitCode) {
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "Process '{0}{1}' exited with code {2}", Executable, null == Arguments ? "" : " " + Arguments, process.ExitCode);
			}
		}
	}
}
