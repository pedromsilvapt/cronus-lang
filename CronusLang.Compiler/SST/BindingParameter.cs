using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST
{
    public class BindingParameter : Node
    {
        #region Semantic Nodes

        public SST.TypeNode Type { get; set; }

        public SST.Identifier Identifier { get; set; }

        #endregion

        public BindingParameter(SymbolsScope scope, AST.BindingParameter syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Type = transformer.ToSST<SST.TypeNode>(scope, syntaxNode.Type);
            Identifier = transformer.ToSST<SST.Identifier>(scope, syntaxNode.Identifier);
        }

        public override int CountChildren() => 2;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Type;
            childrenReceiver[1] = Identifier;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            // TODO Nothing to analyze? Noice
        }
    }
}
