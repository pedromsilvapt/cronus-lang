using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Logic
{
    public class OrOp : BinaryOperator
    {
        public OrOp(SymbolsScope scope, AST.Operators.Logic.OrOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                Type.Resolve(context, context.BoolType);
            }
        }
    }
}
