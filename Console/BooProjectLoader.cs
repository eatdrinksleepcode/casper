using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Casper.IO;

namespace Casper {
	public abstract class BooProjectLoader {

		public static ProjectBase LoadProject(FileInfo scriptPath) {
			return LoadProject(scriptPath, null);
		}

		public static ProjectBase LoadProject(FileInfo scriptPath, ProjectBase parent) {
			var loader = new BooProjectFileLoader(scriptPath, parent);
			return loader.Load();
		}

		public static ProjectBase LoadProject(TextReader scriptContents) {
			var loader = new BooProjectStringLoader(scriptContents);
			return loader.Load();
		}

		private class BooProjectFileLoader : BooProjectLoader {
			private readonly FileInfo scriptPath;

			public BooProjectFileLoader(FileInfo scriptPath, ProjectBase parent) 
				: base(parent) {
				this.scriptPath = scriptPath;
			}

			protected override ICompilerInput GetCompilerInput() {
				return new FileInput(scriptPath.ToString());
			}

			protected override BaseClassStep GetBaseClassStep() {
				return new BaseClassStep(scriptPath.Directory);
			}
		}

		private class BooProjectStringLoader : BooProjectLoader {
			private readonly TextReader scriptContents;

			public BooProjectStringLoader(TextReader scriptContents)
				: base(null) {
				this.scriptContents = scriptContents;
			}

			protected override ICompilerInput GetCompilerInput() {
				return new ReaderInput("content", scriptContents);
			}

			protected override BaseClassStep GetBaseClassStep() {
				return new BaseClassStep(new DirectoryInfo(Directory.GetCurrentDirectory())); // HACK: current directory?
			}
		}

		private readonly ProjectBase parent;

		public BooProjectLoader(ProjectBase parent) {
			this.parent = parent;
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

		private static ProjectBase CreateProjectFromProjectType(ProjectBase parent, Type projectType) {
			var project = (ProjectBase)Activator.CreateInstance(projectType, new object[] {
				parent,
				new RealFileSystem(),
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
