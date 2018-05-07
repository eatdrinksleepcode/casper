using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Casper.IO;

namespace Casper {
	public class BooProjectLoader : IProjectLoader {

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

		private readonly IFileSystem fileSystem;
		private readonly string scriptFile;

		public BooProjectLoader(IFileSystem fileSystem, string scriptFile) {
			this.fileSystem = fileSystem;
			this.scriptFile = scriptFile;
		}

		public ProjectBase LoadProject(string projectPath) {
			var project = Load(fileSystem.Directory(projectPath).File(scriptFile), fileSystem, null);
			project.ConfigureAll(this);
			return project;
		}

		public ProjectBase LoadProject(string projectPath, ProjectBase parent) {
			return Load(parent.Directory(projectPath).File(scriptFile), parent, null);
		}

		public ProjectBase LoadProject(string projectPath, ProjectBase parent, string name) {
			return Load(parent.Directory(projectPath).File(scriptFile), parent, name);
		}

		private ProjectBase Load(IFile scriptPath, params object[] args) {
			if (!scriptPath.Exists()) {
				return null;
			}
			var project = CreateProjectFromProjectType(CompileToProjectType(scriptPath), args);
			return project;
		}

		private Type CompileToProjectType(IFile scriptPath) {
			var projectInput = new FileInput(scriptPath);
			var baseClassStep = new BaseClassStep(scriptPath.Directory);

			return CompileToProjectType(projectInput, baseClassStep);
		}

		private ProjectBase CreateProjectFromProjectType(Type projectType, object[] args) {
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
