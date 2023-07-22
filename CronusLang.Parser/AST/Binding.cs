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
        
        public Expressions.Block Block { get; protected set; }

        public Binding(Identifier identifier, BindingType? type, IList<Binding> bindings, Node expression, LocationSpan blockLocation, LocationSpan location) : base(location)
        {
            Identifier = identifier;
            Signature = type;
            Block = new Expressions.Block(bindings, expression, blockLocation, createScope: false);
        }

        public override int CountChildren()
        {
            return 1 + (Signature != null ? 1 : 0) + 1;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Identifier;

            int offset = 0;
            if (Signature != null)
            {
                childrenReceiver[1] = Signature!; // TODO send null?
                offset += 1;
            }

            childrenReceiver[1 + offset] = Block;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            BindingType? signature = null;
            Expressions.Block block;

            if (newChildren[1] is BindingType)
            {
                signature = ((BindingType)newChildren[1]);
                block = (Expressions.Block)newChildren[2];
            } 
            else
            {
                block = (Expressions.Block)newChildren[1];
            }
            
            return new Binding((Identifier)newChildren[0], signature, block.Bindings, block.Expression, block.Location, Location);
        }
    }
}
