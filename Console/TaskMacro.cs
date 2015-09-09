using Boo.Lang.Compiler.Ast;
using Casper;
using System.Collections.Generic;

public class TaskMacro : Boo.Lang.Compiler.AbstractAstGeneratorMacro {

	public override Statement Expand(MacroStatement macro) {
		return null;
	}

	public override IEnumerable<Node> ExpandGenerator(MacroStatement macro) {
		var identifier = macro.Arguments[0] as ReferenceExpression;
		string taskName = null;
		Expression taskType = Expression.Lift(typeof(Task));
		List<Expression> args = new List<Expression> { Expression.Lift(macro.Body) };
		ExpressionPairCollection namedArgs = null;
		if (null != identifier) {
			taskName = identifier.Name;
		}
		var methodCall = macro.Arguments[0] as MethodInvocationExpression;
		if (null != methodCall) {
			taskName = methodCall.Target.ToString();
			namedArgs = methodCall.NamedArguments;
			if (!methodCall.Arguments.IsEmpty) {
				taskType = methodCall.Arguments[0];
				args.Clear();
			}
		}
		var declaration = new Declaration(taskName, TypeReference.Lift(taskType));
		var declarationStatement = new DeclarationStatement(macro.LexicalInfo, declaration, new MethodInvocationExpression(macro.LexicalInfo, taskType, args.ToArray()) {
			NamedArguments = namedArgs
		});
		yield return declarationStatement;
		var methodInvocationExpression = new MethodInvocationExpression(macro.LexicalInfo, new ReferenceExpression(macro.LexicalInfo, "AddTask"), Expression.Lift(taskName), new ReferenceExpression(macro.LexicalInfo, taskName));
		yield return methodInvocationExpression;
	}
}
