using Boo.Lang.Compiler.Ast;

public class IncludeMacro : Boo.Lang.Compiler.AbstractAstMacro {
	public override Statement Expand(MacroStatement macro) {
		return new ExpressionStatement(macro.LexicalInfo, new MethodInvocationExpression(macro.LexicalInfo, new ReferenceExpression(macro.LexicalInfo, "LoadSubProject"), macro.Arguments.ToArray()));
	}
}
