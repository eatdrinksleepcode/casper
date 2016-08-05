using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Casper.IO;
using CommandLine;
using CommandLine.Text;

namespace Casper {
	public static class MainClass {

		class Options {
			[Value(0, Required = true, MetaName = "script", HelpText = "The relative path of the Casper script to execute")]
			public string ScriptPath { get; set; }

			[Value(1, Required = false, MetaName = "task1 [task2 ...]", HelpText = "The tasks to execute")]
			public IEnumerable<string> TasksToExecute { get; set; }

			[Option]
			public bool Tasks { get; set; }

			[Option]
			public bool Projects { get; set; }

			[Usage]
			public static IEnumerable<Example> Usage {
				get {
					yield return new Example("Execute a task", new Options { ScriptPath = "script", TasksToExecute = new [] { "task1" } } );
					yield return new Example("Execute multiple tasks", new Options { ScriptPath = "script", TasksToExecute = new [] { "task1", "task2" } } );
				}
			}
		}

		public static int Main(string[] args) {
			var timer = Stopwatch.StartNew();
			try {
				var arguments = Parser.Default.ParseArguments<Options>(args);
				return Run(arguments);
			} catch (CasperException ex) {
				WriteError(ex.Message);
				return ex.ExitCode;
			} catch (Exception ex) {
				WriteError(ex);
				return CasperException.EXIT_CODE_UNHANDLED_EXCEPTION;
			} finally {
				Console.WriteLine();
				Console.WriteLine("Total time: {0}", timer.Elapsed);
			}
		}

		static int Run(ParserResult<Options> arguments) {
			return arguments.MapResult(o =>  {
				var project = BooProjectLoader.LoadProject(o.ScriptPath, RealFileSystem.Instance);
				if(o.Tasks) {
					foreach(var task in project.Tasks) {
						Console.Error.WriteLine("{0} - {1}", task.Name, task.Description);
					}
				} else if(o.Projects) {
					var allProjects = Enumerable.Repeat(project, 1).FindAllProjects();
					foreach(var p in allProjects) {
						Console.Error.WriteLine(p.PathDescription);
					}
				} else {
					TaskExecutionGraph taskGraph;
					try {
						taskGraph = project.BuildTaskExecutionGraph(o.TasksToExecute);
					} catch(UnknownTaskException ex) {
						throw new CasperException(CasperException.EXIT_CODE_MISSING_TASK, ex);
					}
					taskGraph.ExecuteTasks();
					Console.WriteLine();
					WriteLine(ConsoleColor.Green, Console.Out, "BUILD SUCCESS");
				}
				return 0;
			}, errors =>  {
				return errors.Any(e => e.Tag == ErrorType.HelpRequestedError) ? 0 : CasperException.EXIT_CODE_UNHANDLED_EXCEPTION;
			});
		}

		public static IEnumerable<ProjectBase> FindAllProjects(this IEnumerable<ProjectBase> projects) {
			var flattenedList = projects;
			foreach(var element in projects) {
				projects = flattenedList.Concat(
					element.Projects.FindAllProjects()
				);
			}
			return projects;
		}

		static void WriteError(object whatWentWrong) {
			Console.Error.WriteLine();
			WriteLine(ConsoleColor.Red, Console.Error, "BUILD FAILURE");
			Console.Error.WriteLine();
			Console.Error.WriteLine("* What went wrong:");
			Console.Error.WriteLine(whatWentWrong);
		}

		static void WriteLine(ConsoleColor consoleColor, System.IO.TextWriter writer, object message) {
			try {
				Console.ForegroundColor = consoleColor;
				writer.WriteLine(message);
			}
			finally {
				Console.ResetColor();
			}
		}

	}
}
