using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Literals
{
    public class DecimalLiteral : Literal<decimal>
    {
        public DecimalLiteral(SymbolsScope scope, AST.Literals.DecimalLiteral syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            Type.Resolve(context, context.DecimalType);
        }
    }
}
