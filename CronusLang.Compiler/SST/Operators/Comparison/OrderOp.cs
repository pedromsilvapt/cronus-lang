using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Comparison
{
    public abstract class OrderOp : BinaryOperator
    {
        #region Semantic Properties

        public SemanticProperty<TypeDefinition> OperationType { get; set; }

        #endregion

        public OrderOp(SymbolsScope scope, AST.Operators.BinaryOperator syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
            OperationType = new SemanticProperty<TypeDefinition>(this, nameof(OperationType));
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                Type.Resolve(context, context.BoolType);
            }

            if (!OperationType.IsResolved)
            {
                if (Left.Type.TrackDependency(context) && Right.Type.TrackDependency(context))
                {
                    var leftType = Left.Type.Value;
                    var rightType = Right.Type.Value;

                    TypeDefinition opType;

                    if ((leftType, rightType) == (context.IntType, context.IntType)) opType = context.IntType;
                    else if ((leftType, rightType) == (context.IntType, context.DecimalType)) opType = context.DecimalType;
                    else if ((leftType, rightType) == (context.DecimalType, context.IntType)) opType = context.DecimalType;
                    else if ((leftType, rightType) == (context.DecimalType, context.DecimalType)) opType = context.DecimalType;
                    else
                    {
                        throw new Exception($"Invalid order operation between {leftType.Symbol.FullPath} and {rightType.Symbol.FullPath}");
                    }

                    OperationType.Resolve(context, opType);
                }
            }
        }
    }
}
