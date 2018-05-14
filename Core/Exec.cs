using System.Diagnostics;
using System.Text;
using System;
using Casper.IO;

namespace Casper {
	public class Exec : TaskBase {
		private static readonly object outputLock = new object();
		
		public string WorkingDirectory { get; set; }
		public string Executable { get; set; }
		public string Arguments { get; set; }
		public bool ShowOutput { get; set; }

		private static readonly Encoding DefaultStreamEncoding = Encoding.UTF8;

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
					StandardOutputEncoding = DefaultStreamEncoding,
					StandardErrorEncoding = DefaultStreamEncoding,
				};
				process = StartProcessAndWatch(allOutput, processStartInfo);
			} else {
				var processStartInfo = new ProcessStartInfo {
					FileName = "cmd.exe",
					Arguments = $"/c {Executable} {Arguments}",
					WorkingDirectory = WorkingDirectory,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					StandardOutputEncoding = DefaultStreamEncoding,
					StandardErrorEncoding = DefaultStreamEncoding,
				};
				process = StartProcessAndWatch(allOutput, processStartInfo);
			}
			process.WaitForExit();
			if (0 != process.ExitCode) {
				if (!ShowOutput) {
					Console.Error.WriteLine(allOutput.ToString());
				}
				throw new CasperException(CasperException.KnownExitCode.TaskFailed, $"Process '{Executable}{ArgumentsDescription}'{WorkingDirectoryDescription} exited with code {process.ExitCode}");
			}
		}

		private string WorkingDirectoryDescription => (null == WorkingDirectory ? "" : " in '" + WorkingDirectory + "'");

		private string ArgumentsDescription => (null == Arguments ? "" : " " + Arguments);

		Process StartProcessAndWatch(StringBuilder allOutput, ProcessStartInfo processStartInfo) {
			var process = Process.Start(processStartInfo);
			Debug.Assert(process != null, nameof(process) + " != null", "The started process should never be null since UseShellExecute is false.");
			process.ErrorDataReceived += (sender, e) => {
				if(e.Data != null) {
					var indentedData = IndentPrefix + e.Data;
					if(ShowOutput) {
						lock (outputLock) {
							Console.Error.WriteLine(indentedData);
						}
					} 
					allOutput.AppendLine(indentedData);
				}
			};
			process.OutputDataReceived += (sender, e) => {
				if(e.Data != null) {
					var indentedData = IndentPrefix + e.Data;
					if(ShowOutput) {
						lock (outputLock) {
							Console.Out.WriteLine(indentedData);
						}
					}
					allOutput.AppendLine(indentedData);
				}
			};
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return process;
		}

		private const string IndentPrefix = "\t";
	}
}
