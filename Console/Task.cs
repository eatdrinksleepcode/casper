using Boo.Lang;

namespace Casper {
	public class Task : TaskBase {
		private readonly ICallable body;

		public Task(ICallable body) {
			this.body = body;
		}

		public override void Execute() {
			this.body.Call(null);
		}
	}
}

