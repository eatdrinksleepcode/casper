using Boo.Lang;
using System.Collections.Generic;


namespace Casper {
	public static class Script {

		private class Task {
			public ICallable Body { get; set; }
		}

		private static HashSet<Task> tasks = new HashSet<Task>();
		
		public static void task(ICallable body) {
			tasks.Add(new Task { Body = body });
		}

		public static void RunAll() {
			foreach (var task in tasks) {
				task.Body.Call(null);
			}
		}
	}
}
