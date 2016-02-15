using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Casper.IO;

namespace Casper {
	public class BaseClassStep : AbstractTransformerCompilerStep {

		private readonly IDirectory location;
	
		public BaseClassStep(IDirectory location) {
			this.location = location;
		}
		
		public override void Run() {
			base.Visit(CompileUnit);
		}

		public override void OnModule(Module node) {
			if (node.Namespace == null) {
				var baseClass = new ClassDefinition(node.LexicalInfo);
				baseClass.Name = node.Name + "Project";
				baseClass.BaseTypes.Add(TypeReference.Lift(typeof(ProjectBase)));
				var configureMethod = new Method(node.LexicalInfo) {
					Name = "Configure",
					Modifiers = TypeMemberModifiers.Override | TypeMemberModifiers.Public,
					Body = node.Globals,
				};
				baseClass.Members.Add(configureMethod);
				baseClass.Members.Add(CreateConstructor(node, ConstructorParameterForRootProject(node)));
				baseClass.Members.Add(CreateConstructor(node, ConstructorParameterForSubProject(node)));
				node.Globals = null;
				node.Members.Add(baseClass);
			}
		}

		private Constructor CreateConstructor(Module node, ParameterDeclaration constructorParameter) {
			var constructor = new Constructor(node.LexicalInfo);
			constructor.Parameters.Add(constructorParameter);
			constructor.Body.Add(new MethodInvocationExpression(
				node.LexicalInfo,
				new SuperLiteralExpression(node.LexicalInfo),
				new ReferenceExpression(node.LexicalInfo, constructorParameter.Name),
				Expression.Lift(this.location.Path)
			));
			return constructor;
		}

		private static ParameterDeclaration ConstructorParameterForRootProject(Module node) {
			return new ParameterDeclaration(node.LexicalInfo) {
				Name = "fileSystem",
				Type = TypeReference.Lift(typeof(IFileSystem))
			};
		}

		private static ParameterDeclaration ConstructorParameterForSubProject(Module node) {
			return new ParameterDeclaration(node.LexicalInfo) {
				Name = "parent",
				Type = TypeReference.Lift(typeof(ProjectBase))
			};
		}
	}
}
