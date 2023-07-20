using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class Binding : Node
    {
        public Identifier Identifier { get; protected set; }

        public BindingType? Signature { get; protected set; }

        public IList<Binding> Bindings { get; protected set; }

        public Node Expression { get; protected set; }

        public Binding(Identifier identifier, BindingType? type, IList<Binding> bindings, Node expression, LocationSpan location) : base(location)
        {
            Identifier = identifier;
            Signature = type;
            Bindings = bindings;
            Expression = expression;
        }

        public override int CountChildren()
        {
            return 1 + 1 + Bindings.Count + 1;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Identifier;
            childrenReceiver[1] = Signature!; // TODO send null?

            for (int i = 0; i < Bindings.Count; i++)
            {
                childrenReceiver[2 + i] = Bindings[i];
            }
            childrenReceiver[childrenReceiver.Length - 1] = Expression;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            Binding[] childBindings = new Binding[newChildren.Length - 1];

            for (int i = 2; i < newChildren.Length - 1; i++)
            {
                childBindings[i - 2] = (Binding)newChildren[i];
            }

            return new Binding((Identifier)newChildren[0], (BindingType?)newChildren[1], childBindings, newChildren[newChildren.Length - 1], Location);
        }
    }
}
