using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Casper.IO;
using System.IO;
using System;

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
				: base(parent, projectFile.Directory.FullPath, name) {
			}
		}

		private class ProjectInfo {
			public string Name { get; internal set; }
			public ProjectBase Project { get; internal set; }
			public IFile ProjectFile { get; internal set; }
		}

		private class SolutionConfigurator : IDisposable {
			private readonly ProjectBase rootProject;
			private readonly IFile solutionFile;
			private readonly Microsoft.Build.Evaluation.ProjectCollection projects;


			public SolutionConfigurator(ProjectBase rootProject, string solutionFilePath) {
				this.rootProject = rootProject;
				this.solutionFile = rootProject.File(solutionFilePath);
				this.projects = new Microsoft.Build.Evaluation.ProjectCollection();
			}

			public void Dispose() {
				if(null != this.projects) {
					this.projects.Dispose();
				}
			}

			public void Configure() {
				var projectInfos = (from line in solutionFile.ReadAllLines()
									let match = crackProjectLine.Match(line)
									where match.Success
									let name = match.Groups["PROJECTNAME"].Value.Trim()
									let projectFile = solutionFile.Directory.File(match.Groups["RELATIVEPATH"].Value.Trim().Replace('\\', System.IO.Path.DirectorySeparatorChar))
									where name != "Solution Items"
									// TODO: also filter out based on project type (GUID)
									where projectFile.Exists()
									select new ProjectInfo { Name = name, ProjectFile = projectFile, Project = GetOrCreateProject(rootProject, name, projectFile) }).ToArray();

				foreach(var p in projectInfos) {
					Configure(p.Project, p.ProjectFile, projectInfos);
				}
			}

			private static ProjectBase GetOrCreateProject(ProjectBase parent, string name, IFile projectFile) {
				ProjectBase result;
				if(!parent.Projects.TryGet(name, out result)) {
					result = new CSharpProject(parent, projectFile, name);
				}
				return result;
			}

			private void Configure(ProjectBase project, IFile projectFile, IEnumerable<ProjectInfo> projectInfos) {
				if(project.Tasks.Count > 0) {
					return;
				}

				IEnumerable<ProjectInfo> dependencies = LoadDependenciesFromProjectFile(projectFile, projectInfos);

				foreach(var d in dependencies) {
					Configure(d.Project, d.ProjectFile, projectInfos);
				}

				project.AddTask("Compile", new MSBuild {
					DependsOn = dependencies.Select(d => d.Project.Tasks["Compile"]).ToList(),
					ProjectFile = projectFile.FullPath,
					Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
					{ Environment.IsUnix ? "BuildingInsideVisualStudio" : "BuildProjectReferences", Environment.IsUnix },
				}
				});

				project.AddTask("Clean", new MSBuild {
					Targets = new[] { "Clean" },
					ProjectFile = projectFile.FullPath,
					Properties = new Dictionary<string, object> {
					{ "Configuration", "Release" },
				},
				});
			}

			private IEnumerable<ProjectInfo> LoadDependenciesFromProjectFile(IFile projectFile, IEnumerable<ProjectInfo> projectInfos) {
				Microsoft.Build.Evaluation.Project projectModel = LoadProject(projectFile);

				return from r in projectModel.GetItems("ProjectReference") join d in projectInfos
						// TODO: match on project file path instead of / in addition to name
						on r.GetMetadataValue("Name") equals d.Name
					   select d;
			}

			private Microsoft.Build.Evaluation.Project LoadProject(IFile projectFile) {
				// Mono's ProjectCollection.LoadString(string fileName) has a bug that causes the project to never actually get loaded
				using(var reader = XmlReader.Create(projectFile.FullPath)) {
					var properties = new Dictionary<string, string> {
					// LoadProject does not resolve relative paths relative to the project directory the way that MSBuild does
						{ "SolutionDir", solutionFile.Directory.FullPath + Path.DirectorySeparatorChar }
				};
					return this.projects.LoadProject(reader, properties, null);
				}
			}
		}

		public static void ConfigureFromSolution(this ProjectBase rootProject, string solutionFilePath) {
			new SolutionConfigurator(rootProject, solutionFilePath).Configure();
		}
	}
}
