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
			if(node.Namespace == null) {
				var baseClass = new ClassDefinition(node.LexicalInfo);
				baseClass.Name = node.Name + "Project";
				baseClass.BaseTypes.Add(TypeReference.Lift(typeof(ProjectBase)));
				var configureMethod = new Method(node.LexicalInfo) {
					Name = "Configure",
					Modifiers = TypeMemberModifiers.Override | TypeMemberModifiers.Protected,
					Body = node.Globals,
				};
				baseClass.Members.Add(configureMethod);
				baseClass.Members.Add(CreateConstructor(node, ConstructorParameterForRootProject(node)));
				baseClass.Members.Add(CreateConstructor(node, ConstructorParameterForSubProject(node)));
				baseClass.Members.Add(CreateConstructor(node, ConstructorParameterForSubProject(node), ConstructorParameterForName(node)));
				node.Globals = null;
				node.Members.Add(baseClass);
			}
		}

		private Constructor CreateConstructor(Module node, params ParameterDeclaration[] constructorParameters) {
			var constructor = new Constructor(node.LexicalInfo);
			foreach(var c in constructorParameters) {
				constructor.Parameters.Add(c);
			}

			var args = constructorParameters.Select(p => new ReferenceExpression(node.LexicalInfo, p.Name)).Cast<Expression>().ToList();
			args.Insert(1, Expression.Lift(this.location.FullPath));

			constructor.Body.Add(new MethodInvocationExpression(
				node.LexicalInfo,
				new SuperLiteralExpression(node.LexicalInfo),
				args.ToArray()
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

		private static ParameterDeclaration ConstructorParameterForName(Module node) {
			return new ParameterDeclaration(node.LexicalInfo) {
				Name = "name",
				Type = TypeReference.Lift(typeof(string))
			};
		}
	}
}
