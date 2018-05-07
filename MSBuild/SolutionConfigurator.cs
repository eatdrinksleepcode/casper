using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Casper.IO;
using System.IO;
using System;

namespace Casper {
	public class SolutionConfigurator : IDisposable {

		private class CSharpProject : ProjectBase {

			public CSharpProject(ProjectBase parent, IFile projectFile, string name)
				: base(parent, projectFile.Directory.FullPath, name) {
				Console.WriteLine($"Loading empty project at '{projectFile.Directory.FullPath}'");
			}
		}

		private class ProjectInfo {
			public string Name { get; internal set; }
			public ProjectBase Project { get; internal set; }
			public IFile ProjectFile { get; internal set; }
		}

		private readonly ProjectBase rootProject;
		private readonly IProjectLoader loader;
		private readonly IFile solutionFile;
		private readonly Microsoft.Build.Evaluation.ProjectCollection projects;

		public SolutionConfigurator(ProjectBase rootProject, string solutionFilePath, IProjectLoader loader) {
			this.rootProject = rootProject;
			this.loader = loader;
			this.solutionFile = rootProject.File(solutionFilePath);
			this.projects = new Microsoft.Build.Evaluation.ProjectCollection();
		}

		public void Dispose() {
			projects?.Dispose();
		}

		public void Configure() {
			var file = solutionFile;
			var projectInfos = (from project in ProjectParser.ExtractProjectsFromSolution(file)
				select new ProjectInfo {
					Name = project.Item1,
					ProjectFile = project.Item2,
					Project = GetOrCreateProject(rootProject, project.Item1, project.Item2)
				}).ToArray();

			foreach (var p in projectInfos) {
				Configure(p.Project, p.ProjectFile, projectInfos);
			}
		}

		private ProjectBase GetOrCreateProject(ProjectBase parent, string name, IFile projectFile) {
			if (!parent.Projects.TryGet(name, out var result)) {
				result = loader?.LoadProject(projectFile.Directory.FullPath, parent, name) ?? new CSharpProject(parent, projectFile, name);
			}

			return result;
		}

		private void Configure(ProjectBase project, IFile projectFile, IReadOnlyCollection<ProjectInfo> projectInfos) {
			if (project.Tasks.Count > 0) {
				return;
			}

			var dependencies = LoadDependenciesFromProjectFile(projectFile, projectInfos).ToList();

			foreach (var d in dependencies) {
				Configure(d.Project, d.ProjectFile, projectInfos);
			}

			project.AddTask("Compile", new MSBuild {
				DependsOn = dependencies.Select(d => d.Project.Tasks["Compile"]).ToList(),
				ProjectFile = projectFile.FullPath,
				Properties = new Dictionary<string, object> {
					{"Configuration", "Release"},
					{"BuildProjectReferences", false},
				}
			});

			project.AddTask("Clean", new MSBuild {
				Targets = new[] {"Clean"},
				ProjectFile = projectFile.FullPath,
				Properties = new Dictionary<string, object> {
					{"Configuration", "Release"},
				},
			});
		}

		private IEnumerable<ProjectInfo> LoadDependenciesFromProjectFile(IFile projectFile,
			IEnumerable<ProjectInfo> projectInfos) {
			var projectModel = LoadProject(projectFile);

			return from r in projectModel.GetItems("ProjectReference")
				join d in projectInfos
					// TODO: match on project file path instead of / in addition to name
					on r.GetMetadataValue("Name") equals d.Name
				select d;
		}

		private Microsoft.Build.Evaluation.Project LoadProject(IFile projectFile) {
			// Mono's ProjectCollection.LoadString(string fileName) has a bug that causes the project to never actually get loaded
			using (var reader = XmlReader.Create(projectFile.FullPath)) {
				var properties = new Dictionary<string, string> {
					// LoadProject does not resolve relative paths relative to the project directory the way that MSBuild does
					{"SolutionDir", solutionFile.Directory.FullPath + Path.DirectorySeparatorChar}
				};
				return projects.LoadProject(reader, properties, null);
			}
		}
	}
}
