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

		private static BooProject LoadProject(FileInfo scriptPath, ProjectBase parent) {
			var project = CompileScript(scriptPath, parent);
			ConfigureProject(project);
			return project;
		}

		private static BooProject CompileScript(FileInfo scriptPath, ProjectBase parent) {
			var projectType = CompileToProjectType(scriptPath);
			var project = CreateProjectFromProjectType(parent, projectType);
			return project;
		}

		private static Type CompileToProjectType(FileInfo scriptPath) {
			var projectInput = new FileInput(scriptPath.ToString());
			var baseClassStep = new BaseClassStep(scriptPath.Directory);

			return CompileToProjectType(projectInput, baseClassStep);
		}

		public BooProject LoadSubProject(FileInfo scriptPath) {
			return LoadProject(scriptPath, this);
		}

		public static BooProject LoadProject(TextReader scriptContents) {
			var project = CreateProjectFromProjectType(null, CompileToProjectType(scriptContents));
			ConfigureProject(project);
			return project;
		}

		private static Type CompileToProjectType(TextReader scriptContents) {
			var projectInput = new ReaderInput("content", scriptContents);
			var baseClassStep = new BaseClassStep(new DirectoryInfo(Directory.GetCurrentDirectory())); // HACK: current directory?

			return CompileToProjectType(projectInput, baseClassStep);
		}

		private static void ConfigureProject(ProjectBase project) {
			try {
				project.Configure();
			}
			catch (TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}

		private static BooProject CreateProjectFromProjectType(ProjectBase parent, Type projectType) {
			var project = (BooProject)Activator.CreateInstance(projectType, new object[] {
				parent
			});
			return project;
		}

		private static Type CompileToProjectType(ICompilerInput projectInput, BaseClassStep baseClassStep) {
			var compileParams = new CompilerParameters();
			compileParams.GenerateInMemory = true;
			compileParams.References.Add(Assembly.GetExecutingAssembly());
			compileParams.References.Add(typeof(TaskBase).Assembly);
			compileParams.References.Add(typeof(MSBuild).Assembly);
			compileParams.References.Add(typeof(NUnit).Assembly);
			compileParams.Input.Add(projectInput);
			compileParams.OutputAssembly = Guid.NewGuid().ToString() + ".dll";
			var context = new CompilerContext(compileParams);
			var pipeline = new CompileToMemory();
			pipeline.Insert(1, baseClassStep);
			pipeline.Run(context);
			if (context.Errors.Count > 0) {
				throw new CasperException(CasperException.EXIT_CODE_COMPILATION_ERROR, context.Errors.ToString());
			}
			var projectType = context.GeneratedAssembly.GetTypes().First();
			return projectType;
		}
	}
}
