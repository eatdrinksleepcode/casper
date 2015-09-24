using Boo.Lang.Compiler.Ast;
using System.IO;

public class IncludeMacro : Boo.Lang.Compiler.AbstractAstMacro {
	public override Statement Expand(MacroStatement macro) {
		var arguments = macro.Arguments.ToArray();
		arguments[0] = new MethodInvocationExpression(macro.LexicalInfo, Expression.Lift(typeof(FileInfo)), new [] { arguments[0] });
		return new ExpressionStatement(macro.LexicalInfo, new MethodInvocationExpression(macro.LexicalInfo, new ReferenceExpression(macro.LexicalInfo, "LoadSubProject"), arguments));
	}
}
