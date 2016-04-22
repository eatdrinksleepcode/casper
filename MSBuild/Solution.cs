using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Casper.IO;

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
			var projects = (from line in solutionFile.ReadAllLines()
							let match = crackProjectLine.Match(line)
							where match.Success
							let name = match.Groups["PROJECTNAME"].Value.Trim()
							let projectFile = solutionFile.Directory.File(match.Groups["RELATIVEPATH"].Value.Trim().Replace('\\', System.IO.Path.DirectorySeparatorChar))
			                where name != "Solution Items"
			                // TODO: also filter out based on project type (GUID)
			                where projectFile.Exists()
			                select new ProjectInfo { Name = name, ProjectFile = projectFile, Project = GetOrCreateProject(rootProject, name, projectFile) }).ToArray();

			foreach(var p in projects) {
				Configure(p.Project, p.ProjectFile, projects);
			}
		}

		private static ProjectBase GetOrCreateProject(ProjectBase parent, string name, IFile projectFile) {
			ProjectBase result;
			if(!parent.Projects.TryGet(name, out result)) {
				result = new CSharpProject(parent, projectFile, name);
			}
			return result;
		}

		private static void Configure(ProjectBase project, IFile projectFile, IEnumerable<ProjectInfo> projects) {
			if(project.Tasks.Count > 0) {
				return;
			}

			IEnumerable<ProjectInfo> dependencies = LoadDependenciesFromProjectFile(projectFile, projects);

			foreach(var d in dependencies) {
				Configure(d.Project, d.ProjectFile, projects);
			}

			project.AddTask("Compile", new MSBuild {
				DependsOn = dependencies.Select(d => d.Project.Tasks["Compile"]).ToList(),
				Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
					{ Environment.IsUnix ? "BuildingInsideVisualStudio" : "BuildProjectReferences", Environment.IsUnix },
				}
			});

			project.AddTask("Clean", new MSBuild {
				Targets = new[] { "Clean" },
				Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
				},
			});
		}

		static IEnumerable<ProjectInfo> LoadDependenciesFromProjectFile(IFile projectFile, IEnumerable<ProjectInfo> projects) {
			IEnumerable<ProjectInfo> dependencies;
			// Mono's ProjectCollection.LoadString(string fileName) has a bug that causes the project to never actually get loaded
			Microsoft.Build.Evaluation.Project projectModel;
			using(var engine = new Microsoft.Build.Evaluation.ProjectCollection()) {
				using(var reader = XmlReader.Create(projectFile.Path)) {
					projectModel = engine.LoadProject(reader);
				}
			}

			var projectReferences = projectModel.GetItems("ProjectReference");

			dependencies = (from r in projectReferences
							join d in projects
			                	// TODO: match on project file path instead of / in addition to name
			                	on r.GetMetadataValue("Name") equals d.Name
							select d).ToArray();
			return dependencies;
		}
	}
}
