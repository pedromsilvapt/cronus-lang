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
    public class Binding : Node
    {
        #region Semantic Nodes

        public SST.Identifier Identifier { get; protected set; }

        public SST.BindingType? Signature { get; set; } = null;

        public SST.Expressions.Block Block { get; set; }

        #endregion

        #region Semantic Properties

        public SemanticProperty<TypeDefinition> Type { get; set; }

        #endregion

        protected bool _exprTypeChecked { get; set; } = false;

        public Binding(SymbolsScope scope, AST.Binding syntaxNode, SemanticTransformer transformer) 
            : base(scope.CreateChild(syntaxNode.Identifier.Name, global: false), syntaxNode)
        {
            // The identifier and the signature portion of the binding should always be analzyed
            // with the original parent scope
            Identifier = transformer.ToSST<SST.Identifier>(Scope.ParentScope!, syntaxNode.Identifier);
            Signature = transformer.TryToSST<SST.BindingType>(Scope.ParentScope!, syntaxNode.Signature);
            // The newly created child scope is only used for the child bindings and the
            // expression of the binding
            Block = transformer.ToSST<SST.Expressions.Block>(Scope, syntaxNode.Block);
            
            // TODO
            Type = new SemanticProperty<TypeDefinition>(this, nameof(Type));

            ReserveSymbols();
        }

        /// <summary>
        /// Method used to collect the names of symbols declared in this scope
        /// </summary>
        protected void ReserveSymbols()
        {
            if (Signature != null)
            {
                var signature = Signature.GetSyntaxNode<AST.BindingType>();

                foreach (var signatureParameter in signature.Parameters)
                {
                    Scope.Reserve(signatureParameter.Identifier.Name);
                }
            }
        }

        public override int CountChildren() => 1 + (Signature != null ? 1 : 0) + 1;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Identifier;

            int offset = 0;
            if (Signature != null)
            {
                offset++;
                childrenReceiver[1] = Signature!;
            }

            childrenReceiver[1 + offset] = Block;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            if (Signature != null && !Signature.TypeSymbol.IsResolved)
            {
                Signature?.TypeSymbol?.Resolve(context, new Symbol(Scope.FullName ?? "<anonymous>"));
            }

            // When the signature is not null, we do not need to infer the type from the expression
            if (!Type.IsResolved && Signature != null)
            {
                if (Signature.Type.TrackDependency(context, out var type))
                {
                    for (int i = 0; i < Signature.Parameters.Count; i++)
                    {
                        var parameter = Signature.Parameters[i];

                        var paramType = parameter.Type.Type.Value;

                        Scope.Register(
                            identifier: parameter.Identifier.GetSyntaxNode<AST.Identifier>().Name,
                            symbol: SymbolsScopeEntry.CreateParameter(i, paramType)
                        );
                    }

                    Type.Resolve(context, type);
                }
            }

            if (Signature == null && !Type.IsResolved && Block.Type.TrackDependency(context, out var blockType))
            {
                Type.Resolve(context, blockType);
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
