using CronusLang.TypeSystem;
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
                    TypeDefinition opType;

                    if (rightType == context.IntType) opType = context.IntType;
                    else if (rightType == context.DecimalType) opType = context.DecimalType;
                    else
                    {
                        throw new Exception($"Invalid negation operation for {rightType.Symbol.FullPath}");
                    }

                    Type.Resolve(context, opType);
                }
            }
        }
    }
}
