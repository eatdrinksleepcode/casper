using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Casper {
	public static class MainClass {
		const int EXIT_CODE_COMPILATION_ERROR = 1;
		const int EXIT_CODE_UNHANDLED_EXCEPTION = 255;
		
		public static int Main(string[] args) {

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				Console.Error.WriteLine(e.ExceptionObject);
				Environment.Exit(EXIT_CODE_UNHANDLED_EXCEPTION);
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
				Script.RunAll();
			} catch(TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}

			return 0;
		}
	}
}
