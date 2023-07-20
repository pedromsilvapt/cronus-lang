using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Expressions
{
    public class StaticAccessor : Node
    {
        public Node Container { get; protected set; }

        public Identifier Member { get; protected set; }

        public StaticAccessor(Node container, Identifier member, LocationSpan locationSpan) : base(locationSpan)
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
            return new StaticAccessor(newChildren[0], (Identifier)newChildren[1], Location);
        }
    }
}
