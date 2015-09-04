using System;

namespace Casper {
	public class Task : TaskBase {
		private readonly Action body;

		public Task(Action body) {
			this.body = body;
		}

		public override void Execute() {
			this.body();
		}
	}
}

