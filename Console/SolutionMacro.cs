using System.Linq;
using Boo.Lang.Compiler.Ast;

namespace Casper {
    public class SolutionMacro : Boo.Lang.Compiler.AbstractAstMacro {
        public override Statement Expand(MacroStatement macro) {
            return new ExpressionStatement(macro.LexicalInfo, 
                new MethodInvocationExpression(macro.LexicalInfo,
                    new MemberReferenceExpression(macro.LexicalInfo, 
                        new ReferenceExpression { Name = typeof(Solution).FullName },
                        "ConfigureFromSolution"
                    ),
                    macro.Arguments.Prepend(new SelfLiteralExpression(macro.LexicalInfo)).Append(new ReferenceExpression(macro.LexicalInfo) { Name = "loader"}).ToArray()
                )
            );            
        }
    }
}
