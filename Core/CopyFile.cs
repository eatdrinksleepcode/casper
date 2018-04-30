using Casper.IO;

namespace Casper {
	public class CopyFile : TaskBase {
		public string Source { get; set; }
		public string Destination { get; set; }

		public override void Execute(IFileSystem fileSystem) {
			if (null == Source) {
				throw new CasperException(CasperException.KnownExitCode.ConfigurationError, "Must set 'Source'");
			}
			if (null == Destination) {
				throw new CasperException(CasperException.KnownExitCode.ConfigurationError, "Must set 'Destination'");
			}
			fileSystem.File(Source).CopyTo(fileSystem.File(Destination));
		}
	}
}
