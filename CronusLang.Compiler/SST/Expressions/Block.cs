using CronusLang.Parser.AST;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Expressions
{
    public class Block : Expression
    {
        #region Semantic Nodes

        public IList<SST.Binding> Bindings { get; set; }

        public SST.Expression Expression { get; set; }

        #endregion

        public Block(SymbolsScope scope, AST.Expressions.Block syntaxNode, SemanticTransformer transformer)
            : base(
                  syntaxNode.CreateScope
                    ? scope.CreateChild(global: syntaxNode.Global, requireCapture: syntaxNode.RequireCapture)
                    : scope,
                  syntaxNode
            )
        {
            // The newly created child scope is only used for the child bindings and the
            // expression of the binding
            Bindings = transformer.ToSST<SST.Binding>(Scope, syntaxNode.Bindings);
            Expression = transformer.ToSST<SST.Expression>(Scope, syntaxNode.Expression);

            // TODO
            Type = new SemanticProperty<TypeDefinition>(this, nameof(Type));

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

        public override int CountChildren() => Bindings.Count + 1;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Bindings.Count; i++) childrenReceiver[i] = Bindings[i];

            childrenReceiver[Bindings.Count] = Expression;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
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
                        Scope.RegisterBinding(childBindingIdentifier, i, global: false, type);
                    }
                }
            }

            if (!Type.IsResolved && Expression.Type.TrackDependency(context, out var exprType))
            {
                Type.Resolve(context, exprType);
            }

            //if (!_exprTypeChecked &&
            //    Type.IsResolved &&
            //    Expression.Type.IsResolved &&
            //    !Type.Value.IsAssignableFrom(Expression.Type.Value))
            //{
            //    _exprTypeChecked = true;

            //    context.Emit(DiagnosticLevel.Error, "Binding type is not compatible with it's expression.");
            //}
            //if (Signature != null && !Signature.Analyzed)
            //{
            //    context.RegisterDependency(new NodeDependency(Signature));
            //}

            //foreach (var childBinding in Bindings)
            //{
            //    if (!childBinding.Analyzed) context.RegisterDependency(new NodeDependency(childBinding));
            //}
        }
    }
}
