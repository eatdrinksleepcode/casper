using System;
using System.IO;
using System.Linq;
using System.Threading;
using Casper.IO;
using FSharp.Compiler.CodeAnalysis;
using FSharp.Compiler.Interactive;
using Microsoft.FSharp.Core;

namespace Casper {
    public class FSharpProjectLoader : IProjectLoader {
        private readonly IFileSystem instance;
        private readonly string scriptFile;

        public class FSharpScriptProject : ProjectBase {
            public FSharpScriptProject(IFileSystem fileSystem, string location, string name = null) : base(fileSystem, location, name) { }
            public FSharpScriptProject(ProjectBase parent, string location, string name = null) : base(parent, location, name) { }
        }

        public FSharpProjectLoader(IFileSystem instance, string scriptFile) {
            this.instance = instance;
            this.scriptFile = scriptFile;
        }

        public ProjectBase LoadProject(string projectPath, ProjectBase parent) {
            throw new NotImplementedException();
        }

        public ProjectBase LoadProject(string projectPath, ProjectBase parent, string name) {
            throw new NotImplementedException();
        }

        public ProjectBase LoadProject(string projectPath) {
            var projectDirectory = instance.Directory(projectPath);
            var projectFile = projectDirectory.File(scriptFile);
            var project = new FSharpScriptProject(instance, projectDirectory.FullPath);
            var fsiConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
            var inStream = new StringReader("");
            var outStream = new StringWriter();
            var errStream = new StringWriter();
            Shell.FsiEvaluationSession fsiSession;
            try {
                fsiSession = Shell.FsiEvaluationSession.Create(fsiConfig, new[] {"fsi.exe"},
                    inStream, outStream, errStream, FSharpOption<bool>.None,
                    FSharpOption<LegacyReferenceResolver>.None);
            } catch (Exception ex) {
                if (ex.Message.Contains("StopProcessingExn")) {
                    throw new CasperException(CasperException.KnownExitCode.CompilationError, errStream.ToString().Trim());
                } else {
                    throw;
                }
            }

            try {
                fsiSession.AddBoundValue("project", project);
                var (result, diagnostics) = fsiSession.EvalScriptNonThrowing(projectFile.FullPath);
                if (result.IsChoice2Of2) {
                    throw ((FSharpChoice<Unit, Exception>.Choice2Of2) result).Item;
                }
                if (diagnostics.Length > 0) {
                    throw new CasperException(CasperException.KnownExitCode.ConfigurationError, 
                        string.Join(Environment.NewLine, diagnostics.Select(x => x.ToString()))
                    );
                }
            } catch (Shell.FsiCompilationException ex) {
                throw new CasperException(CasperException.KnownExitCode.CompilationError, errStream.ToString().Trim());
            }

            return project;
        }
    }
}