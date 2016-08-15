using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Casper.IO;
using System.IO;

namespace Casper {
	public static class Solution {

		// From MSBuild SolutionParser
		private static readonly Regex crackProjectLine = new Regex
			(
				"^"                                             // Beginning of line
				+ "Project\\(\"(?<PROJECTTYPEGUID>.*)\"\\)"
				+ "\\s*=\\s*"                                    // Any amount of whitespace plus "=" plus any amount of whitespace
				+ "\"(?<PROJECTNAME>.*)\""
				+ "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
				+ "\"(?<RELATIVEPATH>.*)\""
				+ "\\s*,\\s*"                                   // Any amount of whitespace plus "," plus any amount of whitespace
				+ "\"(?<PROJECTGUID>.*)\""
				+ "$"                                           // End-of-line
			);

		private class CSharpProject : ProjectBase {

			public CSharpProject(ProjectBase parent, IFile projectFile, string name) 
				: base(parent, projectFile.Directory.Path, name) {
			}
		}

		private class ProjectInfo {
			public string Name { get; internal set; }
			public ProjectBase Project { get; internal set; }
			public IFile ProjectFile { get; internal set; }
		}

		public static void ConfigureFromSolution(this ProjectBase rootProject, string solutionFilePath) {
			var solutionFile = rootProject.File(solutionFilePath);
			var projectInfos = (from line in solutionFile.ReadAllLines()
							let match = crackProjectLine.Match(line)
							where match.Success
							let name = match.Groups["PROJECTNAME"].Value.Trim()
							let projectFile = solutionFile.Directory.File(match.Groups["RELATIVEPATH"].Value.Trim().Replace('\\', System.IO.Path.DirectorySeparatorChar))
			                where name != "Solution Items"
			                // TODO: also filter out based on project type (GUID)
			                where projectFile.Exists()
			                select new ProjectInfo { Name = name, ProjectFile = projectFile, Project = GetOrCreateProject(rootProject, name, projectFile) }).ToArray();

			using(var projects = new Microsoft.Build.Evaluation.ProjectCollection()) {
				foreach(var p in projectInfos) {
					Configure(p.Project, p.ProjectFile, projects, projectInfos, solutionFile.Directory);
				}
			}
		}

		private static ProjectBase GetOrCreateProject(ProjectBase parent, string name, IFile projectFile) {
			ProjectBase result;
			if(!parent.Projects.TryGet(name, out result)) {
				result = new CSharpProject(parent, projectFile, name);
			}
			return result;
		}

		private static void Configure(ProjectBase project, IFile projectFile, Microsoft.Build.Evaluation.ProjectCollection projects, IEnumerable<ProjectInfo> projectInfos, IDirectory solutionDirectory) {
			if(project.Tasks.Count > 0) {
				return;
			}

			IEnumerable<ProjectInfo> dependencies = LoadDependenciesFromProjectFile(projectFile, projects, projectInfos, solutionDirectory);

			foreach(var d in dependencies) {
				Configure(d.Project, d.ProjectFile, projects, projectInfos, solutionDirectory);
			}

			project.AddTask("Compile", new MSBuild {
				DependsOn = dependencies.Select(d => d.Project.Tasks["Compile"]).ToList(),
				ProjectFile = projectFile.Path,
				Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
					{ Environment.IsUnix ? "BuildingInsideVisualStudio" : "BuildProjectReferences", Environment.IsUnix },
				}
			});

			project.AddTask("Clean", new MSBuild {
				Targets = new[] { "Clean" },
				ProjectFile = projectFile.Path,
				Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
				},
			});
		}

		private static IEnumerable<ProjectInfo> LoadDependenciesFromProjectFile(IFile projectFile, Microsoft.Build.Evaluation.ProjectCollection projects, IEnumerable<ProjectInfo> projectInfos, IDirectory solutionDirectory) {
			Microsoft.Build.Evaluation.Project projectModel = LoadProject(projectFile, projects, solutionDirectory);

			return from r in projectModel.GetItems("ProjectReference") join d in projectInfos
					// TODO: match on project file path instead of / in addition to name
					on r.GetMetadataValue("Name") equals d.Name
				   select d;
		}

		private static Microsoft.Build.Evaluation.Project LoadProject(IFile projectFile, Microsoft.Build.Evaluation.ProjectCollection engine, IDirectory solutionDir) {
			// Mono's ProjectCollection.LoadString(string fileName) has a bug that causes the project to never actually get loaded
			using(var reader = XmlReader.Create(projectFile.Path)) {
				var properties = new Dictionary<string, string> {
					// LoadProject does not resolve relative paths relative to the project directory the way that MSBuild does
					{ "SolutionDir", solutionDir.Path + Path.DirectorySeparatorChar }
				};
				return engine.LoadProject(reader, properties, null );
			}
		}
	}
}
