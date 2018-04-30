using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Casper.IO;

namespace Casper {
	public class BooProjectLoader {

		public static ProjectBase LoadProject(string scriptPath, IFileSystem fileSystem) {
			var loader = new BooProjectLoader(fileSystem.File(scriptPath), fileSystem);
			var project = loader.Load();
			project.ConfigureAll();
			return project;
		}

		public static ProjectBase LoadProject(string scriptPath, ProjectBase parent) {
			var loader = new BooProjectLoader(parent.File(scriptPath), parent);
			return loader.Load();
		}

		public static ProjectBase LoadProject(string scriptPath, ProjectBase parent, string name) {
			var loader = new BooProjectLoader(parent.File(scriptPath), parent, name);
			return loader.Load();
		}

		private class FileInput : ICompilerInput {
			private readonly IFile file;

			public FileInput(IFile file) {
				this.file = file;
			}
			
			public string Name => file.FullPath;

			public TextReader Open() {
				return file.OpenText();
			}
		}

		private readonly IFile scriptPath;
		private readonly object[] args;

		private BooProjectLoader(IFile scriptPath, params object[] args) {
			this.scriptPath = scriptPath;
			this.args = args;
		}

		public ProjectBase Load() {
			var project = CreateProjectFromProjectType(CompileToProjectType());
			return project;
		}

		private Type CompileToProjectType() {
			var projectInput = new FileInput(scriptPath);
			var baseClassStep = new BaseClassStep(scriptPath.Directory);

			return CompileToProjectType(projectInput, baseClassStep);
		}

		private ProjectBase CreateProjectFromProjectType(Type projectType) {
			var project = (ProjectBase)Activator.CreateInstance(projectType, args);
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
				throw new CasperException(CasperException.KnownExitCode.CompilationError, context.Errors.ToString());
			}
			var projectType = context.GeneratedAssembly.GetTypes().First();
			return projectType;
		}
	}
}
