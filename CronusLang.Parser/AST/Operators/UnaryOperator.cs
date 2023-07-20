using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators
{
    public abstract class UnaryOperator : Node
    {
        public Node Right { get; protected set; }

        public UnaryOperator(Node right, Location start) : base(start + right.Location)
        {
            Right = right;
        }

        public override int CountChildren()
        {
            return 1;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Right;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return (Node)Activator.CreateInstance(GetType(), new object[] { newChildren[0] })!;
        }
    }
}
