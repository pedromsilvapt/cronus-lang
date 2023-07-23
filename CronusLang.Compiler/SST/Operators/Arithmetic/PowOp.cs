using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Arithmetic
{
    public class PowOp : BinaryOperator
    {
        public PowOp(SymbolsScope scope, AST.Operators.Arithmetic.PowOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                if (Left.Type.TrackDependency(context, out var leftType) &&
                    Right.Type.TrackDependency(context, out var rightType))
                {
                    TypeDefinition opType;

                    if ((leftType, rightType) == (context.IntType, context.IntType)) opType = context.DecimalType;
                    else if ((leftType, rightType) == (context.IntType, context.DecimalType)) opType = context.DecimalType;
                    else if ((leftType, rightType) == (context.DecimalType, context.IntType)) opType = context.DecimalType;
                    else if ((leftType, rightType) == (context.DecimalType, context.DecimalType)) opType = context.DecimalType;
                    else
                    {
                        throw new Exception($"Invalid power operation between {leftType.Symbol.FullPath} and {rightType.Symbol.FullPath}");
                    }

                    Type.Resolve(context, opType);
                }
            }
        }
    }
}
