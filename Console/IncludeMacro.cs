using Boo.Lang.Compiler.Ast;
using Casper;

public class IncludeMacro : Boo.Lang.Compiler.AbstractAstMacro {
	public override Statement Expand(MacroStatement macro) {
		return new ExpressionStatement(macro.LexicalInfo, new MethodInvocationExpression(macro.LexicalInfo, new MemberReferenceExpression(macro.LexicalInfo, Expression.Lift(typeof(Script)), "CompileAndExecuteScript"), macro.Arguments.ToArray()));
	}
}
