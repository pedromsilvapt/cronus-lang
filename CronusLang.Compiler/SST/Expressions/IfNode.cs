using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Expressions
{
    public class IfNode : Expression
    {
        public Expression Condition { get; protected set; }

        public Expression ThenExpression { get; protected set; }

        public Expression ElseExpression { get; protected set; }

        public IfNode(SymbolsScope scope, AST.Expressions.IfNode syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Condition = transformer.ToSST<SST.Expression>(scope, syntaxNode.Condition);
            ThenExpression = transformer.ToSST<SST.Expression>(scope, syntaxNode.ThenExpression);
            ElseExpression = transformer.ToSST<SST.Expression>(scope, syntaxNode.ElseExpression);
        }

        public override int CountChildren() => 3;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Condition;
            childrenReceiver[1] = ThenExpression;
            childrenReceiver[2] = ElseExpression;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                if (Condition.Type.TrackDependency(context, out var condType))
                {                    
                    if (!condType.IsAssignableTo(context.BoolType))
                    {
                        // TODO Make sure the type is assignable to a boolean
                    }
                }

                if (ThenExpression.Type.TrackDependency(context))
                {
                    Type.Resolve(context, ThenExpression.Type.Value);
                }
            }
        }
    }
}
