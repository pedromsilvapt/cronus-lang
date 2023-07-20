using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Arithmetic
{
    public class NegOp : UnaryOperator
    {
        public NegOp(SymbolsScope scope, AST.Operators.Arithmetic.NegOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                if (Right.Type.TrackDependency(context, out var rightType))
                {
                    var intType = context.IntType;

                    if (rightType == intType)
                    {
                        Type.Resolve(context, intType);
                    }
                    else
                    {
                        // TODO What to do when the type checker finds an error?
                        // Should semantic properties be able to be resolved with errors?
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
