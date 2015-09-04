using System.Collections.Generic;


namespace Casper {
	public class MSBuild : TaskBase {
		public string WorkingDirectory { get; set; }
		public string ProjectFile {	get; set; }
		public string[] Targets { get; set; }

		public override void Execute() {

			List<string> args = new List<string>();
			if (null != ProjectFile) {
				args.Add(ProjectFile);
			}
			if (null != Targets) {
				args.Add("/t:" + string.Join(";", Targets));
			}

			var exec = new Exec {
				WorkingDirectory = WorkingDirectory,
				Executable = "xbuild",
				Arguments = string.Join(" ", args),
			};
			exec.Execute();
		}
	}
}
