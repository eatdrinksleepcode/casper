﻿using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using System.Linq;

namespace Casper {
	public static class MainClass {

		class Options {
			[Value(0, Required = true, MetaName = "script", HelpText = "The relative path of the Casper script to execute")]
			public string ScriptPath { get; set; }

			[Value(1, Required = true, MetaName = "task1 [task2 ...]", HelpText = "The tasks to execute")]
			public IEnumerable<string> Tasks { get; set; }

			[Usage]
			public static IEnumerable<Example> Usage {
				get {
					yield return new Example("Execute a task", new Options { ScriptPath = "script", Tasks = new [] { "task1" } } );
					yield return new Example("Execute multiple tasks", new Options { ScriptPath = "script", Tasks = new [] { "task1", "task2" } } );
				}
			}
		}
		
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

			var arguments = Parser.Default.ParseArguments<Options>(args);
			return arguments.MapResult(
				o => {
					Script.CompileAndExecute(o.ScriptPath, o.Tasks);
					return 0;
				},
				errors => {
					return errors.Any(e => e.Tag == ErrorType.HelpRequestedError) ? 0 : CasperException.EXIT_CODE_UNHANDLED_EXCEPTION;
				}
			);
		}
	}
}
