using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using System.IO;

namespace Casper {
	public abstract class BooProject : ProjectBase {
		protected BooProject(ProjectBase parent, DirectoryInfo location)
			: base(parent, location) {
		}

		public static BooProject LoadProject(FileInfo scriptPath) {
			return LoadProject(scriptPath, null);
		}

		private static BooProject LoadProject(FileInfo scriptPath, BooProject parent) {
			var project = CompileScript(scriptPath, parent);
			try {
				project.Configure();
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
			return project;
		}

		private static BooProject CompileScript(FileInfo scriptPath, BooProject parent) {
			var projectType = CompileToProjectType(scriptPath);
			var project = (BooProject)Activator.CreateInstance(projectType, new object[] {
				parent
			});
			return project;
		}

		public BooProject LoadSubProject(FileInfo scriptPath) {
			return LoadProject(scriptPath, this);
		}

		private static Type CompileToProjectType(FileInfo scriptPath) {
			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.References.Add(typeof(TaskBase).Assembly);
			compileParams.References.Add(typeof(MSBuild).Assembly);
			compileParams.References.Add(typeof(NUnit).Assembly);
			compileParams.Input.Add(new FileInput(scriptPath.ToString()));
			compileParams.OutputAssembly = Guid.NewGuid().ToString() + ".dll";
			var context = new CompilerContext(compileParams);
			var pipeline = new CompileToMemory();
			pipeline.Insert(1, new BaseClassStep(scriptPath.Directory));
			pipeline.Run(context);
			if (context.Errors.Count > 0) {
				throw new CasperException(CasperException.EXIT_CODE_COMPILATION_ERROR, context.Errors.ToString());
			}
			var projectType = context.GeneratedAssembly.GetTypes().First();
			return projectType;
		}
	}
}
