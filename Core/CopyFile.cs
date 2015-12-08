using System.IO;


namespace Casper {
	public class CopyFile : TaskBase {
		public FileInfo Source { get; set; }
		public FileInfo Destination { get; set; }

		public override void Execute() {
			if (null == Source) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Source'");
			}
			if (null == Destination) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Destination'");
			}
			File.Copy(Source.FullName, Destination.FullName, overwrite: true);
		}
	}
}
