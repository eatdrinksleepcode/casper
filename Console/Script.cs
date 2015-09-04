using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.IO;
using System.Reflection;
using System;
using System.Runtime.ExceptionServices;

namespace Casper {
	public static class Script {

		private static Dictionary<string, TaskBase> tasks = new Dictionary<string, TaskBase>();

		private static TaskBase GetTaskByName(string name) {
			TaskBase result;
			if (!tasks.TryGetValue(name, out result)) {
				throw new CasperException(CasperException.EXIT_CODE_MISSING_TASK, "Task '{0}' does not exist", name);
			}
			return result;
		}
			
		private static CompilerContext CompileScript(string scriptPath) {
			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.References.Add(typeof(TaskBase).Assembly);
			compileParams.Input.Add(new FileInput(scriptPath));
			compileParams.OutputAssembly = Guid.NewGuid().ToString() + ".dll";
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

		public static void AddTask(string name, TaskBase task) {
			tasks.Add(name, task);
		}

		public static IEnumerable<KeyValuePair<string, TaskBase>> GetAllTasks() {
			return tasks;
		}

		public static void CompileAndExecuteTasks(string scriptPath, params string[] taskNamesToExecute) {
			CompileAndExecuteTasks(scriptPath, (IEnumerable<string>)taskNamesToExecute);
			
		}

		public static void CompileAndExecuteTasks(string scriptPath, IEnumerable<string> taskNamesToExecute) {
			CompileAndExecuteScript(scriptPath);
			try {
				ExecuteTasks(taskNamesToExecute);
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}

		public static void CompileAndExecuteScript(string scriptPath) {
			var context = CompileScript(scriptPath);
			try {
				ExecuteScript(context);
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
