using System;
using NUnit.Engine;
using System.Xml;
using System.Linq;

namespace Casper {
	public class NUnit : TaskBase {

		private class TestEventListener : ITestEventListener {
			public void OnTestEvent(string report) {
				Console.Error.WriteLine(report);
			}
		}

		public string TestAssembly { get; set; }
		public string TestName { get; set; }

		public override void Execute() {
			if (null == TestAssembly) {
				throw new CasperException(CasperException.EXIT_CODE_CONFIGURATION_ERROR, "Must set 'TestAssembly'");
			}

			var filter = TestFilter.Empty;
			if (null != TestName) {
				filter = new TestFilter("<filter><tests><test>" + TestName + "</test></tests></filter>");
			}
			
			var engine = new TestEngine();
			var runner = engine.GetRunner(new TestPackage(TestAssembly));
			var result = runner.Run(null, filter);

			var failures = Visit(result);
			if (0 < failures) {
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "{0} tests failed", failures);
			}
		}

		private int Visit(XmlNode node) {
			int count = node.Name == "test-case" && GetAttribute(node, "result") == "Failed" ? 1 : 0;
			return count + Visit(node.ChildNodes);
		}

		private int Visit(XmlNodeList nodes)
		{
			return nodes.Cast<XmlNode>().Sum(Visit);
		}

		private static string GetAttribute(XmlNode node, string attributeName) {
			var attr = node.Attributes[attributeName];
			return attr == null ? null : attr.Value;
		}
	}
}
