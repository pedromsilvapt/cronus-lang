using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Literals
{
    public class IntLiteral : Literal<int>
    {
        public IntLiteral(SymbolsScope scope, AST.Literals.IntLiteral syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            Type.Resolve(context, context.IntType);
        }
    }
}
