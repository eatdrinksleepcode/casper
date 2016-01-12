using System;
using NUnit.Engine;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Casper.IO;

namespace Casper {
	public class NUnit : TaskBase {

		public string TestAssembly { get; set; }
		public string TestName { get; set; }

		public override void Execute(IFileSystem fileSystem) {
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

			var failures = TestResultVisitor.CollectErrors(result);
			if (failures.Count > 0) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("Failing tests:");
				Console.Error.WriteLine();
				foreach (var error in failures) {
					Console.Error.WriteLine("{0}: {1}", error.Name, error.Message);
				}
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "{0} tests failed", failures.Count);
			}
		}

		private struct TestError {
			public string Name;
			public string Message;
		}

		private class TestResultVisitor {

			public static List<TestError> CollectErrors(XmlNode results) {
				var visitor = new TestResultVisitor();
				visitor.Visit(results);
				return visitor.errors;
			}
			
			private readonly List<TestError> errors = new List<TestError>();

			private void Visit(XmlNode node) {
				if (node.Name == "test-case" && GetAttribute(node, "result") == "Failed") {
					errors.Add(new TestError { Name = GetAttribute(node, "fullname"), Message = FindErrorForTestCase(node) });
				}
				Visit(node.ChildNodes);
			}

			static string FindErrorForTestCase(XmlNode node) {
				var failureNode = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "failure");
				var messageNode = failureNode?.ChildNodes?.Cast<XmlNode>()?.FirstOrDefault(n => n.Name == "message");
				return messageNode?.InnerText;
			}

			private void Visit(XmlNodeList nodes)
			{
				foreach (var node in nodes.Cast<XmlNode>()) {
					Visit(node);
				}
			}

			private static string GetAttribute(XmlNode node, string attributeName) {
				var attr = node.Attributes[attributeName];
				return attr == null ? null : attr.Value;
			}
		}
	}
}
