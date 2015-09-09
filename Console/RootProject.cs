using System;

namespace Casper {
	public class RootProject : BooProject {

		public RootProject() : base(null, null) { }

		public override void Configure() {
			throw new NotSupportedException();
		}
	}
}
