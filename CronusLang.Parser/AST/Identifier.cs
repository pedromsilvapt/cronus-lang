using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class Identifier : Node
    {
        public string Name { get; protected set; }

        public bool IsExpression { get; set; } = false;

        public Identifier(string name, LocationSpan locationSpan) : base(locationSpan)
        {
            Name = name;
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
