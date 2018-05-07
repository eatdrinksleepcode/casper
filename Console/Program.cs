using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Casper.IO;
using CommandLine;
using CommandLine.Text;

namespace Casper {
	public static class MainClass {

		class Options {
			[Value(0, Required = true, MetaName = "script", HelpText = "The relative path of the Casper script to execute")]
			public string ScriptFile { get; set; }

			[Value(1, Required = false, MetaName = "task1 [task2 ...]", HelpText = "The tasks to execute")]
			public IEnumerable<string> TasksToExecute { get; set; }

			[Option]
			public bool Tasks { get; set; }

			[Option]
			public bool Projects { get; set; }

			[Usage]
			public static IEnumerable<Example> Usage {
				get {
					yield return new Example("Execute a task", new Options { ScriptFile = "script", TasksToExecute = new [] { "task1" } } );
					yield return new Example("Execute multiple tasks", new Options { ScriptFile = "script", TasksToExecute = new [] { "task1", "task2" } } );
				}
			}
		}

		public static int Main(string[] args) {
			var timer = Stopwatch.StartNew();
			try {
				var parser = new Parser(settings => { settings.HelpWriter = Console.Error; });
				var arguments = parser.ParseArguments<Options>(args);
				return (byte) Run(arguments);
			} catch (CasperException ex) {
				WriteError(ex.Message);
				return (byte) ex.ExitCode;
			} catch (Exception ex) {
				WriteError(ex);
				return (byte) CasperException.KnownExitCode.UnhandledException;
			} finally {
				Console.WriteLine();
				Console.WriteLine("Total time: {0}", timer.Elapsed);
			}
		}

		static CasperException.KnownExitCode Run(ParserResult<Options> arguments) {
			return arguments.MapResult(Run, errors =>  {
				return errors.Any(e => e.Tag == ErrorType.HelpRequestedError)
										? CasperException.KnownExitCode.None
										: CasperException.KnownExitCode.InvocationgError;
			});
		}

		static CasperException.KnownExitCode Run(Options o) {
			if(Path.IsPathRooted(o.ScriptFile)) {
				throw new CasperException(CasperException.KnownExitCode.InvocationgError, "ScriptFile must be a relative path");
			}

			var loader = new BooProjectLoader(RealFileSystem.Instance, o.ScriptFile);
			var project = loader.LoadProject(".");
			if (o.Tasks) {
				foreach (var task in project.Tasks) {
					Console.Error.WriteLine("{0} - {1}", task.Name, task.Description);
				}
			} else if (o.Projects) {
				var allProjects = new List<ProjectBase> {project}.FindAllProjects();
				foreach (var p in allProjects) {
					Console.Error.WriteLine(p.PathDescription);
				}
			} else {
				TaskExecutionGraph taskGraph;
				try {
					taskGraph = project.BuildTaskExecutionGraph(o.TasksToExecute);
				} catch (UnknownTaskException ex) {
					throw new CasperException(CasperException.KnownExitCode.MissingTask, ex);
				}

				taskGraph.ExecuteTasks();
				WriteLine(ConsoleColor.Green, Console.Out, "BUILD SUCCESS");
			}

			return CasperException.KnownExitCode.None;
		}

		static IEnumerable<ProjectBase> FindAllProjects(this IReadOnlyCollection<ProjectBase> projects) {
			return projects.Concat(projects.SelectMany(p => p.Projects.FindAllProjects()));
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
