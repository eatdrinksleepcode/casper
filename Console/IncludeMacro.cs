using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;
using Casper;

public class IncludeMacro : Boo.Lang.Compiler.AbstractAstMacro {
	public override Statement Expand(MacroStatement macro) {
		var arguments = new List<Expression> { 
			macro.Arguments[0],
			new SelfLiteralExpression()
		};
		if(macro.Arguments.Count > 1) {
			arguments.Add(macro.Arguments[1]);
		}
		return new ExpressionStatement(macro.LexicalInfo, 
		                               new MethodInvocationExpression(macro.LexicalInfo,
		                                                              new MemberReferenceExpression(macro.LexicalInfo,
		                                                                                            Expression.Lift(typeof(BooProjectLoader)),
		                                                                                            "LoadProject"
		                                                                                           ),
		                                                              arguments.ToArray()
		                                                             )
		                              );
	}
}
