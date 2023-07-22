using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Expressions
{
    public class Block : Node
    {
        public IList<Binding> Bindings { get; protected set; }

        public Node Expression { get; protected set; }

        public bool CreateScope { get; set; }

        public bool Global { get; set; }

        public bool RequireCapture { get; set; }

        public Block(IList<Binding> bindings, Node expression, LocationSpan location, bool createScope = false, bool requireCapture = false, bool global = false) : base(location)
        {
            Bindings = bindings;
            Expression = expression;
            CreateScope = createScope;
            RequireCapture = requireCapture;
            Global = global;
        }

        public override int CountChildren()
        {
            return Bindings.Count + 1;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                childrenReceiver[i] = Bindings[i];
            }

            childrenReceiver[Bindings.Count] = Expression;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            List<Binding> bindings = new List<Binding>(newChildren.Length - 1);
            for (int i = 0; i < newChildren.Length - 1; i++)
            {
                bindings.Add((Binding)newChildren[i]);
            }

            return new Block(bindings, newChildren[newChildren.Length - 1], Location, requireCapture: RequireCapture, global: Global);
        }
    }
}
