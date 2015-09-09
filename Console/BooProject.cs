using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;

namespace Casper {
	public abstract class BooProject : ProjectBase {
		public BooProject(ProjectBase parent, string location) 
			: base(parent, location) {
		}
			
		public BooProject CompileScript(string scriptPath) {
			var projectType = CompileToProjectType(scriptPath);
			var project = (BooProject)Activator.CreateInstance(projectType, new object[] { this });
			return project;
		}

		public BooProject CompileAndExecuteScript(string scriptPath) {
			var project = this.CompileScript(scriptPath);
			try {
				ConfigureProject(project);
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
			return project;
		}

		public void CompileAndExecuteTasks(string scriptPath, params string[] taskNamesToExecute) {
			CompileAndExecuteTasks(scriptPath, (IEnumerable<string>)taskNamesToExecute);
		}

		public void CompileAndExecuteTasks(string scriptPath, IEnumerable<string> taskNamesToExecute) {
			CompileAndExecuteScript(scriptPath);
			try {
				ExecuteTasks(taskNamesToExecute);
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}

		private static void ConfigureProject(BooProject project) {
			project.Configure();
		}

		private static Type CompileToProjectType(string scriptPath) {
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
			pipeline.Insert(1, new BaseClassStep(Path.GetDirectoryName(Path.GetFullPath(scriptPath))));
			pipeline.Run(context);
			if (context.Errors.Count > 0) {
				throw new CasperException(CasperException.EXIT_CODE_COMPILATION_ERROR, context.Errors.ToString());
			}
			var projectType = context.GeneratedAssembly.GetTypes().First();
			return projectType;
		}
	}
}
