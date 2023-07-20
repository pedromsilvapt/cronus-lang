using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class BindingParameter : Node
    {
        public TypeNode Type { get; protected set; }

        public Identifier Identifier { get; protected set; }

        public BindingParameter(TypeNode type, Identifier identifier, LocationSpan location) : base(location)
        {
            Type = type;
            Identifier = identifier;
        }

        public override int CountChildren()
        {
            return 2;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Type;
            childrenReceiver[1] = Identifier;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return new BindingParameter((TypeNode)newChildren[0], (Identifier)newChildren[1], Location);
        }
    }
}
