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

		private static ProjectBase currentProject;
			
		private static CompilerContext CompileScript(string scriptPath) {
			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.References.Add(typeof(TaskBase).Assembly);
			compileParams.References.Add(typeof(MSBuild).Assembly);
			compileParams.References.Add(typeof(NUnit).Assembly);
			compileParams.Input.Add(new FileInput(scriptPath));
			compileParams.OutputAssembly = Guid.NewGuid().ToString() + ".dll";
			var context = new CompilerContext(compileParams);
			var pipeline = new CompileToMemory();
			pipeline.Insert(1, new BaseClassStep());
			pipeline.Run(context);
			if (context.Errors.Count > 0) {
				throw new CasperException(CasperException.EXIT_CODE_COMPILATION_ERROR, context.Errors.ToString());
			}
			return context;
		}

		private static void ExecuteScript(CompilerContext context) {
			var projectType = context.GeneratedAssembly.GetTypes().First();
			var oldProject = currentProject;
			try {
				currentProject = (ProjectBase)Activator.CreateInstance(projectType, new object[] { Script.GetCurrentProject()});
				currentProject.Configure();
			} finally {
				if (null != oldProject) {
					currentProject = oldProject;
				}
			}
		}

		private static void ExecuteTasks(IEnumerable<string> taskNamesToExecute) {
			var tasks = taskNamesToExecute.Select(a => currentProject.GetTaskByName(a)).ToArray();
			var taskGraphClosure = tasks.SelectMany(t => t.AllDependencies()).Distinct().ToArray();
			Array.Sort(taskGraphClosure, (t1, t2) => t1.AllDependencies().Contains(t2) ? 1 : t2.AllDependencies().Contains(t1) ? -1 : 0);
			foreach (var task in taskGraphClosure) {
				Console.WriteLine(task.Name + ":");
				task.Execute();
			}
		}

		public static void AddTask(string name, TaskBase task) {
			currentProject.AddTask(name, task);
		}

		public static TaskCollection GetCurrentTasks() {
			return currentProject.Tasks;
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
			currentProject = null;
		}

		public static ProjectBase GetCurrentProject() {
			return currentProject;
		}
	}
}
