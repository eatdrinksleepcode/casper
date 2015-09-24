using System.Collections.Generic;

namespace Casper
{
	public class ProjectCollection
	{
		private readonly ProjectBase parent;
		private readonly Dictionary<string, ProjectBase> subprojects = new Dictionary<string, ProjectBase>();

		public ProjectCollection(ProjectBase parent) {
			this.parent = parent;
		}

		public void Add(ProjectBase project) {
			this.subprojects.Add(project.Name, project);
		}

		public ProjectBase this[string name] {
			get {
				ProjectBase result;
				if (!subprojects.TryGetValue(name, out result)) {
					throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Project '{0}' does not exist in {1}", name, parent.PathDescription);
				}
				return result;
			}
		}
	}
}
