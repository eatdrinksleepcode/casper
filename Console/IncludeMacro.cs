using Boo.Lang.Compiler.Ast;
using System.IO;
using Casper;

public class IncludeMacro : Boo.Lang.Compiler.AbstractAstMacro {
	public override Statement Expand(MacroStatement macro) {
		var arguments = new Expression[] { 
			new MethodInvocationExpression(macro.LexicalInfo, Expression.Lift(typeof(FileInfo)), new [] { macro.Arguments[0] }), 
			new SelfLiteralExpression() 
		};
		return new ExpressionStatement(macro.LexicalInfo, new MethodInvocationExpression(macro.LexicalInfo, new MemberReferenceExpression(macro.LexicalInfo, Expression.Lift(typeof(BooProjectLoader)), "LoadProject"), arguments));
	}
}
