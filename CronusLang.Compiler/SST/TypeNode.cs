using CronusLang.Compiler.Dependencies;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST
{
    public class TypeNode : Node
    {
        #region Segmantic Nodes

        public IList<SST.Identifier> Symbol { get; protected set; }

        public IList<SST.TypeNode> Generics { get; protected set; }

        #endregion

        #region Semantic Properties

        public SemanticProperty<TypeDefinition> Type { get; set; }

        #endregion

        #region Properties

        public IEnumerable<string> SymbolSegments => Symbol.Select(id => id.GetSyntaxNode<AST.Identifier>().Name);

        #endregion

        public TypeNode(SymbolsScope scope, AST.TypeNode syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Symbol = transformer.ToSST<SST.Identifier>(scope, syntaxNode.Symbol);
            Generics = transformer.ToSST<SST.TypeNode>(scope, syntaxNode.Generics);

            // TODO
            Type = new SemanticProperty<TypeDefinition>(this, nameof(Type));
        }

        public override int CountChildren() => Symbol.Count + Generics.Count;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Symbol.Count; i++) childrenReceiver[i] = Symbol[i];
            for (int i = 0; i < Generics.Count; i++) childrenReceiver[i] = Generics[i + Symbol.Count];
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            // TODO Generics not supported!
            // TODO Types inside namespaces/other types not supported!

            var symbol = new TypeSystem.Symbol(SymbolSegments);

            var symbolIdentifier = symbol.Segments.First();

            // TODO Use the full symbol for lookup instead of just the first semgent
            var symbolValue = Scope.TryLookup(symbolIdentifier);

            if (symbolValue == null)
            {
                context.RegisterDependency(new SymbolDependency(Scope, symbol.Segments.First()));
            } 
            else
            {
                if (!symbolValue.IsType)
                {
                    throw new Exception($"Symbol {symbolIdentifier} is not a symbol.");
                }


                var type = symbolValue.Type;

                Type.Resolve(context, type!);
            }
        }
    }
}
