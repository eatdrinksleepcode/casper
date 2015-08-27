using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Linq;

namespace Casper {
	public static class MainClass {
		const int EXIT_CODE_COMPILATION_ERROR = 1;
		const int EXIT_CODE_MISSING_TASK = 2;
		const int EXIT_CODE_UNHANDLED_EXCEPTION = 255;
		
		public static int Main(string[] args) {

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				var casperEx = e.ExceptionObject as CasperException;
				if(null != casperEx) {
					Console.Error.WriteLine(casperEx.Message);
					Environment.Exit(casperEx.ExitCode);
				} else {
					Console.Error.WriteLine(e.ExceptionObject);
					Environment.Exit(EXIT_CODE_UNHANDLED_EXCEPTION);
				}
			};

			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.Input.Add(new FileInput(args[0]));
			var context = new CompilerContext(compileParams);
			new CompileToMemory().Run(context);

			if (context.Errors.Count > 0) {
				Console.Error.WriteLine(context.Errors);
				return EXIT_CODE_COMPILATION_ERROR;
			}

			try {
				context.GeneratedAssembly.EntryPoint.Invoke(null, new Object[] { new String[0] });
				var tasks = args.Skip(1).Select(a => GetTaskByName(a)).ToArray();
				var taskGraphClosure = tasks.SelectMany(t => t.AllDependencies()).Distinct().ToArray();
				Array.Sort(taskGraphClosure, (t1, t2) => t1.AllDependencies().Contains(t2) ? 1 : t2.AllDependencies().Contains(t1) ? -1 : 0);
				foreach(var task in taskGraphClosure) {
					task.Execute();
				}
			} catch(TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}

			return 0;
		}

		static Script.Task GetTaskByName(string taskName) {
			var task = Script.GetTaskByName(taskName);
			if (null == task) {
				throw new CasperException(EXIT_CODE_MISSING_TASK, "Task '{0}' does not exist", taskName);
			}
			return task;
		}

		private class CasperException : Exception {
			private readonly int exitCode;

			public CasperException(int exitCode, string message, params object[] args)
				: base(string.Format(message, args)) {
				this.exitCode = exitCode;
			}

			public int ExitCode {
				get {
					return exitCode;
				}
			}
		}
	}
}
