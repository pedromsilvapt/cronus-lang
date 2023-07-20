using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Expressions
{
    public class IfNode : Node
    {
        public Node Condition { get; protected set; }

        public Node ThenExpression { get; protected set; }

        public Node ElseExpression { get; protected set; }

        public IfNode(Node condition, Node thenExpression, Node elseExpression, LocationSpan location) : base(location)
        {
            Condition = condition;
            ThenExpression = thenExpression;
            ElseExpression = elseExpression;
        }

        public override int CountChildren()
        {
            return 3;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Condition;
            childrenReceiver[1] = ThenExpression;
            childrenReceiver[2] = ElseExpression;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return new IfNode(newChildren[0], newChildren[1], newChildren[2], Location);
        }
    }
}
