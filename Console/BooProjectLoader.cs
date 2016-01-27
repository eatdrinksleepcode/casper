using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Pipelines;
using Casper.IO;

namespace Casper {
	public abstract class BooProjectLoader {

		public static ProjectBase LoadProject(string scriptPath) {
			return LoadProject(scriptPath, (ProjectBase)null);
		}

		public static ProjectBase LoadProject(string scriptPath, IFileSystem fileSystem) {
			return LoadProject(scriptPath, null, fileSystem);
		}

		public static ProjectBase LoadProject(string scriptPath, ProjectBase parent) {
			return LoadProject(scriptPath, parent, RealFileSystem.Instance);
		}

		private static ProjectBase LoadProject(string scriptPath, ProjectBase parent, IFileSystem fileSystem) {
			var loader = new BooProjectFileLoader(fileSystem.File(scriptPath), parent, fileSystem);
			return loader.Load();
		}

		private class FileInput : ICompilerInput {
			private readonly IFile file;

			public FileInput(IFile file) {
				this.file = file;
			}
			
			public string Name {
				get { return this.file.Path; }
			}

			public TextReader Open() {
				return file.OpenText();
			}
		}

		private class BooProjectFileLoader : BooProjectLoader {
			private readonly IFile scriptPath;

			public BooProjectFileLoader(IFile scriptFile, ProjectBase parent, IFileSystem fileSystem) 
				: base(parent, fileSystem) {
				this.scriptPath = scriptFile;
			}

			protected override ICompilerInput GetCompilerInput() {
				return new FileInput(scriptPath);
			}

			protected override BaseClassStep GetBaseClassStep() {
				return new BaseClassStep(scriptPath.Directory);
			}
		}

		private readonly ProjectBase parent;
		private readonly IFileSystem fileSystem;

		public BooProjectLoader(ProjectBase parent, IFileSystem fileSystem) {
			this.parent = parent;
			this.fileSystem = fileSystem;
		}

		public ProjectBase Load() {
			var project = CreateProjectFromProjectType(parent, CompileToProjectType());
			project.Configure();
			return project;
		}
			
		private Type CompileToProjectType() {
			var projectInput = GetCompilerInput();
			var baseClassStep = GetBaseClassStep();

			return CompileToProjectType(projectInput, baseClassStep);
		}

		protected abstract ICompilerInput GetCompilerInput();

		protected abstract BaseClassStep GetBaseClassStep();

		private ProjectBase CreateProjectFromProjectType(ProjectBase parent, Type projectType) {
			var project = (ProjectBase)Activator.CreateInstance(projectType, new object[] {
				parent,
				fileSystem,
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
