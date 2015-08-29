using Boo.Lang;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.IO;
using System.Reflection;
using System;
using System.Runtime.ExceptionServices;
using System.Collections;

namespace Casper {
	public static class Script {

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

		private static Dictionary<string, Task> tasks = new Dictionary<string, Task>();

		private static Task GetTaskByName(string name) {
			Task result;
			if (!tasks.TryGetValue(name, out result)) {
				throw new CasperException(CasperException.EXIT_CODE_MISSING_TASK, "Task '{0}' does not exist", name);
			}
			return result;
		}
			
		private static CompilerContext CompileScript(string scriptPath) {
			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.Input.Add(new FileInput(scriptPath));
			var context = new CompilerContext(compileParams);
			new CompileToMemory().Run(context);
			if (context.Errors.Count > 0) {
				throw new CasperException(CasperException.EXIT_CODE_COMPILATION_ERROR, context.Errors.ToString());
			}
			return context;
		}

		private static void ExecuteScript(CompilerContext context) {
			context.GeneratedAssembly.EntryPoint.Invoke(null, new object[] {
				new string[0]
			});
		}

		private static void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
			var tasks = taskNamesToExecute.Select(a => GetTaskByName(a)).ToArray();
			var taskGraphClosure = tasks.SelectMany(t => t.AllDependencies()).Distinct().ToArray();
			Array.Sort(taskGraphClosure, (t1, t2) => t1.AllDependencies().Contains(t2) ? 1 : t2.AllDependencies().Contains(t1) ? -1 : 0);
			foreach (var task in taskGraphClosure) {
				task.Execute();
			}
		}

		public static void AddTask(string name, Task task) {
			tasks.Add(name, task);
		}

		public static void CompileAndExecute(string scriptPath, params string[] taskNamesToExecute) {
			CompileAndExecute(scriptPath, (IEnumerable<string>)taskNamesToExecute);
			
		}

		public static void CompileAndExecute(string scriptPath, IEnumerable<string> taskNamesToExecute) {
			var context = CompileScript(scriptPath);
			try {
				ExecuteScript(context);
				ExecuteTasks(taskNamesToExecute);
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}

		public static void Reset() {
			tasks.Clear();
		}
	}
}
