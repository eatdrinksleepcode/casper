using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Casper.IO;

namespace Casper {

	public abstract class TaskBase {
		private IList<TaskBase> dependencies = new List<TaskBase>();

		public abstract void Execute(IFileSystem fileSystem);

		public IEnumerable<TaskBase> AllDependencies() {
			return Enumerable.Repeat(this, 1).Concat(dependencies.SelectMany(d => d.AllDependencies()));
		}

		public IList DependsOn { 
			get { return (IList)dependencies; }
			set { dependencies = value.Cast<TaskBase>().ToList() ?? new List<TaskBase>(); }
		}

		public string Description { get; set; }

		public string Name { get; internal set; }

		public string Path { get { return Project.PathPrefix + Name ; } }

		public ProjectBase Project { get; set; }
	}
}
