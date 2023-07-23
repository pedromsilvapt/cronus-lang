using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Literals
{
    public class BoolLiteral : Literal<bool>
    {
        public BoolLiteral(SymbolsScope scope, AST.Literals.BoolLiteral syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            Type.Resolve(context, context.BoolType);
        }
    }
}
