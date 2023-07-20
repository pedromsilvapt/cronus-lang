using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Expressions
{
    public class Application : Node
    {
        public Node Func { get; protected set; }

        public IList<Node> Args { get; protected set; }

        public Application(Node func, IList<Node> args) : base(func.Location + args.Last().Location)
        {
            Func = func;
            Args = args;
        }

        public override int CountChildren()
        {
            return 1 + Args.Count;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Func;
            for (int i = 0; i < Args.Count; i++)
            {
                childrenReceiver[1 + i] = Args[i];
            }
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            Node[] args = new Node[newChildren.Length - 1];
            for (int i = 1; i < newChildren.Length; i++)
            {
                args[i - 1] = newChildren[i];
            }

            return new Application(newChildren[0], args);
        }
    }
}
