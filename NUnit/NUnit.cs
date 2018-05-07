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
			if(null == TestAssembly) {
				throw new CasperException(CasperException.KnownExitCode.ConfigurationError, "Must set 'TestAssembly'");
			}

			var filter = TestFilter.Empty;
			if(null != TestName) {
				filter = new TestFilter("<filter><test>" + TestName + "</test></filter>");
			}

			XmlNode result;
			using(var engine = new TestEngine())
			using(var runner = engine.GetRunner(new TestPackage(TestAssembly))) {
				result = runner.Run(null, filter);
			}

			var outputFileName = fileSystem.File(TestAssembly).Directory.File(Name + (!string.IsNullOrEmpty(TestName) ? "." + TestName : "") + ".nunit").FullPath;
			using(var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true })) {
				result.WriteTo(writer);
			}

			var failures = TestResultVisitor.CollectErrors(result);
			if(failures.Count > 0) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("Failing tests:");
				Console.Error.WriteLine();
				foreach(var error in failures) {
					Console.Error.WriteLine("{0}:", error.Name);
					Console.Error.WriteLine(error.Message);
					Console.Error.WriteLine(error.StackTrace);
				}
				throw new CasperException(CasperException.KnownExitCode.TaskFailed, $"{failures.Count} tests failed");
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
				if(node.Name == "test-suite" && GetAttribute(node, "result") == "Failed" && GetAttribute(node, "site") != "Child"	) {
					var error = new TestError { Name = GetAttribute(node, "name") };
					if(GetAttribute(node, "label") == "Error") {
						FindErrorForTestCase(node, error);
					} else {
						FindErrorForTestSuite(node, error);
					}
					errors.Add(error);
				} else if(node.Name == "test-case" && GetAttribute(node, "result") == "Failed") {
					var error = new TestError { Name = GetAttribute(node, "fullname") };
					FindErrorForTestCase(node, error);
					errors.Add(error);
				}
				Visit(node.ChildNodes);
			}

			private static void FindErrorForTestSuite(XmlNode node, TestError error) {
				var failureNode = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "reason");
				error.Message = GetChildNode(failureNode, "message")?.InnerText;
			}

			private static void FindErrorForTestCase(XmlNode node, TestError error) {
				var failureNode = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "failure");
				error.Message = GetChildNode(failureNode, "message")?.InnerText;
				error.StackTrace = GetChildNode(failureNode, "stack-trace")?.InnerText;
			}

			static XmlNode GetChildNode(XmlNode parentNode, string childNodeName) {
				return parentNode?.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == childNodeName);
			}

			private void Visit(XmlNodeList nodes) {
				foreach(var node in nodes.Cast<XmlNode>()) {
					Visit(node);
				}
			}

			private static string GetAttribute(XmlNode node, string attributeName) {
				return node.Attributes?[attributeName]?.Value;
			}
		}
	}
}
