using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Casper.IO;

namespace Casper {
	public class MSBuild : TaskBase {
		public string WorkingDirectory { get; set; }
		public string ProjectFile {	get; set; }
		public IList Targets { get; set; }
		public IDictionary Properties { get; set; }

		public override void Execute(IFileSystem fileSystem) {

			List<string> args = new List<string>();
			if (null != ProjectFile) {
				args.Add("\"" + ProjectFile + "\"");
			}
			if (null != Targets) {
				args.Add("/t:" + string.Join(";", Targets.Cast<object>().Select(t => t.ToString())));
			}
			if (null != Properties) {
				foreach (var propertyName in Properties.Keys) {
					args.Add("/p:" + propertyName + "=" + Properties[propertyName]);
				}
			}

			var exec = new Exec {
				WorkingDirectory = WorkingDirectory,
				Executable = Environment.IsMono ? "xbuild" : "msbuild",
				Arguments = string.Join(" ", args),
			};
			exec.Execute(fileSystem);
		}
	}
}
