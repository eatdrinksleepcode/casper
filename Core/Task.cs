using System;
using Casper.IO;

namespace Casper {
	public class Task : TaskBase {
		private readonly Action body;

		public Task(Action body) {
			this.body = body;
		}

		public override void Execute(IFileSystem fileSystem) {
			body();
		}
	}
}

