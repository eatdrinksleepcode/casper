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
				var constructor = new Constructor(node.LexicalInfo);
				constructor.Parameters.Add(new ParameterDeclaration(node.LexicalInfo) {
					Name = "parent",
					Type = TypeReference.Lift(typeof(ProjectBase))
				});
				constructor.Parameters.Add(new ParameterDeclaration(node.LexicalInfo) {
					Name = "fileSystem",
					Type = TypeReference.Lift(typeof(IFileSystem))
				});
				constructor.Body.Add(new MethodInvocationExpression(
					node.LexicalInfo,
					new SuperLiteralExpression(node.LexicalInfo),
					new ReferenceExpression(node.LexicalInfo, "parent"),
					Expression.Lift(this.location.Path),
					new ReferenceExpression(node.LexicalInfo, "fileSystem")
				));
				baseClass.Members.Add(constructor);
				node.Globals = null;
				node.Members.Add(baseClass);
			}
		}
	}
}
