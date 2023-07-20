using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Literals
{
    public abstract class Literal<T> : Node
    {
        public T Value { get; protected set; }

        public Literal(T value, LocationSpan locationSpan) : base(locationSpan)
        {
            Value = value;
        }

        public override int CountChildren()
        {
            return 0;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            // Do nothing
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return this;
        }
    }
}
