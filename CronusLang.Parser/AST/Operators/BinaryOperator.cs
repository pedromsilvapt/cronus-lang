using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators
{
    public abstract class BinaryOperator : Node
    {

        public Node Left { get; protected set; }

        public Node Right { get; protected set; }

        public BinaryOperator(Node left, Node right) : base(left.Location + right.Location)
        {
            Left = left;
            Right = right;
        }

        public override int CountChildren()
        {
            return 2;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Left;
            childrenReceiver[1] = Right;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return (Node)Activator.CreateInstance(GetType(), new object[] { newChildren[0], newChildren[1] })!;
        }
    }
}
