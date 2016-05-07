using System;

namespace Casper {
	public class UnknownTaskException : Exception {
		public UnknownTaskException(ProjectBase project, string taskName)
			: base(string.Format("Task '{0}' does not exist in {1}", taskName, project.PathDescription)) {
		}
	}
}
