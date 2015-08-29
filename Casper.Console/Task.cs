using Boo.Lang;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Casper {

	public class Task {
		private readonly ICallable body;
		private IEnumerable<Task> dependencies = Enumerable.Empty<Task>();

		public Task(ICallable body) {
			this.body = body;
		}

		public void Execute() {
			this.body.Call(null);
		}

		public IEnumerable<Task> AllDependencies() {
			return Enumerable.Repeat(this, 1).Concat(dependencies.SelectMany(d => d.AllDependencies()));
		}

		public IEnumerable dependsOn { set { dependencies = value.Cast<Task>() ?? Enumerable.Empty<Task>(); } }
	}
}
