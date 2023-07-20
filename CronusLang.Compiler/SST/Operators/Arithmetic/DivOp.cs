using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Arithmetic
{
    public class DivOp : BinaryOperator
    {
        public DivOp(SymbolsScope scope, AST.Operators.Arithmetic.DivOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                if (Left.Type.TrackDependency(context, out var leftType) &&
                    Right.Type.TrackDependency(context, out var rightType))
                {
                    var intType = context.IntType;

                    if (leftType == intType && rightType == intType)
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
