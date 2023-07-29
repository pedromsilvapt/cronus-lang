using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CronusLang.TypeSystem;
using CronusLang.TypeSystem.Types;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST
{
    public class BindingType : Node
    {
        #region Semantic Nodes

        public IList<SST.BindingParameter> Parameters { get; protected set; }

        public SST.TypeNode ReturnType { get; protected set; }

        #endregion

        #region Semantic Properties

        public SemanticProperty<SymbolIdentifier> TypeSymbol { get; set; }

        public SemanticProperty<TypeDefinition> Type { get; protected set; }

        #endregion

        public BindingType(SymbolsScope scope, AST.BindingType syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Parameters = transformer.ToSST<SST.BindingParameter>(scope, syntaxNode.Parameters);
            ReturnType = transformer.ToSST<SST.TypeNode>(scope, syntaxNode.ReturnType);

            TypeSymbol = new SemanticProperty<SymbolIdentifier>(this, nameof(TypeSymbol));
            Type = new SemanticProperty<TypeDefinition>(this, nameof(Type));
        }

        public override int CountChildren() => Parameters.Count + 1;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Parameters.Count; i++) childrenReceiver[i] = Parameters[i];
            childrenReceiver[Parameters.Count] = ReturnType;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            if (TypeSymbol.TrackDependency(context, out var symbol))
            {
                // Function Binding
                if (Parameters.Any())
                {
                    // Wait until all the types are resolved
                    if (Parameters.All(p => p.Type.Type.TrackDependency(context)) &&
                        ReturnType.Type.TrackDependency(context))
                    {
                        TypeDefinition returnType = ReturnType.Type.Value;
                        List<FunctionTypeArgumentDefinition> arguments = Parameters
                            .Select(par => new FunctionTypeArgumentDefinition(par.Type.Type.Value))
                            .ToList();

                        var fnType = new FunctionTypeDefinition(symbol, arguments, returnType);

                        context.Types.Register(fnType);

                        Type.Resolve(context, fnType);
                    }
                }
                // Constant Binding
                else
                {
                    // Wait until all the types are resolved
                    if (ReturnType.Type.TrackDependency(context))
                    {
                        TypeDefinition returnType = ReturnType.Type.Value;

                        Type.Resolve(context, returnType);
                    }
                }
            }
        }
    }
}
