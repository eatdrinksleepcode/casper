using Boo.Lang.Compiler.Ast;
using Casper;
using System.Collections.Generic;
using System.Linq;

public class TaskMacro : Boo.Lang.Compiler.AbstractAstGeneratorMacro {

	#region implemented abstract members of AbstractAstGeneratorMacro

	public override Statement Expand(MacroStatement macro) {
		return null;
	}

	public override System.Collections.Generic.IEnumerable<Node> ExpandGenerator(MacroStatement macro) {
		var identifier = macro.Arguments[0] as ReferenceExpression;
		string taskName = null;
		ExpressionPairCollection namedArgs = null;
		if (null != identifier) {
			taskName = identifier.Name;
		}
		var methodCall = macro.Arguments[0] as MethodInvocationExpression;
		if (null != methodCall) {
			taskName = methodCall.Target.ToString();
			namedArgs = methodCall.NamedArguments;
		}
		var declaration = new Declaration(taskName, new SimpleTypeReference(macro.LexicalInfo, "Casper.Script.Task"));
		var declarationStatement = new DeclarationStatement(macro.LexicalInfo, declaration, new MethodInvocationExpression(macro.LexicalInfo, Expression.Lift(typeof(Casper.Script.Task)), Expression.Lift(macro.Body)) {
			NamedArguments = namedArgs
		});
		yield return declarationStatement;
		var methodInvocationExpression = new MethodInvocationExpression(macro.LexicalInfo, new MemberReferenceExpression(macro.LexicalInfo, Expression.Lift(typeof(Script)), "AddTask"), Expression.Lift(taskName), new ReferenceExpression(macro.LexicalInfo, taskName));
		yield return methodInvocationExpression;
	}

	#endregion
}
