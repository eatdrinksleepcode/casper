using Boo.Lang;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Casper {

	public abstract class TaskBase {
		private IEnumerable<TaskBase> dependencies = Enumerable.Empty<TaskBase>();

		public abstract void Execute();

		public IEnumerable<TaskBase> AllDependencies() {
			return Enumerable.Repeat(this, 1).Concat(dependencies.SelectMany(d => d.AllDependencies()));
		}

		public IEnumerable DependsOn { set { dependencies = value.Cast<TaskBase>() ?? Enumerable.Empty<TaskBase>(); } }
	}
}
