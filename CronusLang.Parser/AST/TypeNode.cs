using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class TypeNode : Node
    {
        public IList<Identifier> Symbol { get; protected set; }

        public IList<TypeNode> Generics { get; protected set; }

        public TypeNode(IList<Identifier> symbol, IList<TypeNode> generics, LocationSpan location) : base(location)
        {
            Symbol = symbol;
            Generics = generics;
        }

        public TypeNode(IList<Identifier> symbol, LocationSpan location) : base(location)
        {
            Symbol = symbol;
            Generics = new List<TypeNode>();
        }

        public static TypeNode Array(IList<Identifier> symbol, LocationSpan location)
        {
            return Array(new TypeNode(symbol, location), location);
        }

        public static TypeNode Array(TypeNode elemType, LocationSpan location)
        {
            var listSymbol = "System.Collections.Generic.IList"
                .Split(".")
                .Select(id => new Identifier(id, location))
                .ToList();

            return new TypeNode(listSymbol, new List<TypeNode> { elemType }, location);
        }

        public override int CountChildren()
        {
            return Symbol.Count + Generics.Count;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Symbol.Count; i++)
            {
                childrenReceiver[i] = Symbol[i];
            }
            for (int i = 0; i < Generics.Count; i++)
            {
                childrenReceiver[Symbol.Count + i] = Generics[i];
            }
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }
    }
}
