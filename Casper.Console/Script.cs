using Boo.Lang;
using System.Collections.Generic;


namespace Casper {
	public static class Script {

		public class Task {
			private ICallable body;
			private Task dependency;

			public void Execute() {
				if (null != dependency) {
					dependency.Execute();
				}
				this.body.Call(null);
			}

			public void Act(ICallable body) {
				this.body = body;
			}

			public void AddDependency(Task dependency) {
				this.dependency = dependency;
			}
		}

		private static Dictionary<string, Task> tasks = new Dictionary<string, Task>();
		private static Task currentTask;
		
		public static Task task(string name, ICallable body) {
			try {
				currentTask = new Task();
				body.Call(null);
				tasks.Add(name, currentTask);
				return currentTask;
			} finally {
				currentTask = null;
			}
		}

		public static void act(ICallable body) {
			currentTask.Act(body);
		}

		public static void dependsOn(Task dependency) {
			currentTask.AddDependency(dependency);
		}

		public static Task GetTaskByName(string name) {
			Task result;
			return tasks.TryGetValue(name, out result) ? result : null;
		}
	}
}
