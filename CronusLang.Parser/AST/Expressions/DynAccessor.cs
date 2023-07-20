using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Expressions
{
    public class DynAccessor : Node
    {
        public Node Container { get; protected set; }

        public Node Member { get; protected set; }

        public DynAccessor(Node container, Node member, LocationSpan locationSpan) : base(locationSpan)
        {
            Container = container;
            Member = member;
        }

        public override int CountChildren()
        {
            return 2;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Container;
            childrenReceiver[1] = Member;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return new DynAccessor(newChildren[0], newChildren[1], Location);
        }
    }
}
