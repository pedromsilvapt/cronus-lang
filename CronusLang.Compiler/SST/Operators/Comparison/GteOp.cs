using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Comparison
{
    public class GteOp : OrderOp
    {
        public GteOp(SymbolsScope scope, AST.Operators.Comparison.GteOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }
    }
}
