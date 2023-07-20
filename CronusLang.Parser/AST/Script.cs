using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class Script : Node
    {
        public IList<Binding> Bindings { get; protected set; }

        public Script(IList<Binding> bindings) : base(LocationSpan.Empty)
        {
            Bindings = bindings;
        }

        public override int CountChildren()
        {
            return Bindings.Count;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                childrenReceiver[i] = Bindings[i];
            }
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            var bindings = new Binding[newChildren.Length];
            for (int i = 0; i < Bindings.Count; i++)
            {
                bindings[i] = (Binding)newChildren[i];
            }
            return new Script(bindings);
        }
    }
}
