using System.Collections;
using System.Collections.Generic;

namespace Casper {
	public class ProjectCollection : IReadOnlyCollection<ProjectBase> {
		private readonly ProjectBase parent;
		private readonly Dictionary<string, ProjectBase> subprojects = new Dictionary<string, ProjectBase>();

		public ProjectCollection(ProjectBase parent) {
			this.parent = parent;
		}

		public void Add(ProjectBase project) {
			this.subprojects.Add(project.Name, project);
		}

		public IEnumerator<ProjectBase> GetEnumerator() {
			return subprojects.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public int Count {
			get {
				return subprojects.Count;
			}
		}

		public ProjectBase this[string name] {
			get {
				ProjectBase result;
				if(!subprojects.TryGetValue(name, out result)) {
					throw new CasperException(CasperException.KnownExitCode.ConfigurationError, "Project '{0}' does not exist in {1}", name, parent.PathDescription);
				}
				return result;
			}
		}

		public bool TryGet(string name, out ProjectBase value) {
			return subprojects.TryGetValue(name, out value);
		}
	}
}
