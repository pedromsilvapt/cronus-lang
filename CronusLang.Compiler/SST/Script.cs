using CronusLang.Parser.AST;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST
{
    public class Script : Node
    {
        public IList<SST.Binding> Bindings { get; protected set; }

        public Script(SymbolsScope scope, AST.Script syntaxNode, SemanticTransformer transformer) : base(scope.CreateChild(), syntaxNode)
        {
            Bindings = transformer.ToSST<SST.Binding>(Scope, syntaxNode.Bindings);

            ReserveSymbols();
        }

        protected Script(SymbolsScope scope, AST.Script syntaxNode, IList<SST.Binding> bindings) : base(scope.CreateChild(), syntaxNode)
        {
            Bindings = bindings;

            ReserveSymbols();
        }

        /// <summary>
        /// Method used to collect the names of symbols declared in this scope
        /// </summary>
        protected void ReserveSymbols()
        {
            foreach (var childBinding in Bindings)
            {
                Scope.Reserve(childBinding.GetSyntaxNode<AST.Binding>().Identifier.Name);
            }
        }

        public override int CountChildren() => Bindings.Count();

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Bindings.Count; i++) childrenReceiver[i] = Bindings[i];
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            List<SST.Binding> bindings = new List<SST.Binding>();

            for (int i = 0; i < newChildren.Length; i++) bindings[i] = (SST.Binding)newChildren[i];

            return new Script(Scope, GetSyntaxNode<AST.Script>(), bindings);
        }

        public override void Analyze(SemanticContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                var childBinding = Bindings[i];

                string childBindingIdentifier = childBinding
                    .GetSyntaxNode<AST.Binding>()
                    .Identifier
                    .Name;

                // If this symbol is not yet registered
                if (Scope.TryLookup(childBindingIdentifier, LookupOptions.SelfOnly) == null)
                {
                    if (childBinding.Type.TrackDependency(context, out var type))
                    {
                        Scope.RegisterBinding(childBindingIdentifier, i, global: true, type);
                    }
                }
            }

        }
    }
}
