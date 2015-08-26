using Boo.Lang;
using System.Collections.Generic;


namespace Casper {
	public static class Script {

		public class Task {
			private readonly ICallable body;

			public Task(ICallable body) {
				this.body = body;
			}

			public void Execute() {
				this.body.Call(null);
			}
		}

		private static Dictionary<string, Task> tasks = new Dictionary<string, Task>();
		
		public static void task(string name, ICallable body) {
			tasks.Add(name, new Task(body));
		}

		public static Task GetTaskByName(string name) {
			Task result;
			return tasks.TryGetValue(name, out result) ? result : null;
		}
	}
}
