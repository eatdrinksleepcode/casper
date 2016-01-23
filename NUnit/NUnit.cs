using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Casper.IO;
using NUnit.Engine;

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
					Console.Error.WriteLine("{0}:\n{1}\n{2}", error.Name, error.Message, error.StackTrace);
				}
				throw new CasperException(CasperException.EXIT_CODE_TASK_FAILED, "{0} tests failed", failures.Count);
			}
		}

		private class TestError {
			public string Name;
			public string Message;
			public string StackTrace;
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
					var error = new TestError { Name = GetAttribute(node, "fullname") };
					FindErrorForTestCase(node, error);
					errors.Add(error);
				}
				Visit(node.ChildNodes);
			}

			private static void FindErrorForTestCase(XmlNode node, TestError error) {
				var failureNode = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "failure");
				error.Message = GetChildNode(failureNode, "message")?.InnerText;
				error.StackTrace = GetChildNode(failureNode, "stack-trace")?.InnerText;
			}

			static XmlNode GetChildNode(XmlNode parentNode, string childNodeName) {
				return parentNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == childNodeName);
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
