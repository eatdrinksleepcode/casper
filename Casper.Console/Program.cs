using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Linq;

namespace Casper {
	public static class MainClass {
		
		public static int Main(string[] args) {

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				var casperEx = e.ExceptionObject as CasperException;
				if(null != casperEx) {
					Console.Error.WriteLine(casperEx.Message);
					Environment.Exit(casperEx.ExitCode);
				} else {
					Console.Error.WriteLine(e.ExceptionObject);
					Environment.Exit(CasperException.EXIT_CODE_UNHANDLED_EXCEPTION);
				}
			};

			var scriptPath = args[0];
			var taskNamesToExecute = args.Skip(1);
			Script.CompileAndExecute(scriptPath, taskNamesToExecute);

			return 0;
		}
	}
}
