using Casper.IO;
using System.Linq;

namespace Casper {
    public class Csc : TaskBase {
        public IDirectory WorkingDirectory { get; set; }
        public string[] SystemReferences { get; set; }
        public IFile OutputFileName { get; set; }

        public override void Execute(IFileSystem fileSystem) {
            var systemReferenceDir = fileSystem.File(typeof(string).Assembly.Location).Directory;
            var frameworkDir = systemReferenceDir.Directory("../../..");
            var cscReferences = SystemReferences.Select(r => "/reference:" + systemReferenceDir.File(r + ".dll").FullPath);

			var exec = new Exec {
                WorkingDirectory = WorkingDirectory.FullPath,
				Executable = frameworkDir.File("Commands/csc").FullPath,
				Arguments = "/recurse:*.cs /target:library /noconfig /nostdlib+ /errorreport:none /debug+ /debug:portable /optimize+ \"/out:" + OutputFileName.FullPath + "\" " + string.Join(" ", cscReferences),
			};
			exec.Execute(fileSystem);
        }
    }
}
