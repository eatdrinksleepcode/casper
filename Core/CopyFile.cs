using System.Collections.Generic;
using Casper.IO;

namespace Casper {
	public class CopyFile : TaskBase {
		public IFile Source { get; set; }
		public IFile Destination { get; set; }

		public override void Execute(IFileSystem fileSystem) {
			if (null == Source) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Source'");
			}
			if (null == Destination) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'Destination'");
			}
			Source.CopyTo(Destination);
		}

		public override IEnumerable<IFile> InputFiles {
			get { yield return Source; }
		}

		public override IEnumerable<IFile> OutputFiles {
			get { yield return Destination; }
		}
	}
}
