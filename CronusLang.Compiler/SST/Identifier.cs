using CronusLang.Compiler.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST
{
    public class Identifier : Expression
    {
        public SemanticProperty<SymbolsScopeEntry> Symbol { get; set; }

        public Identifier(SymbolsScope scope, AST.Identifier syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Symbol = new SemanticProperty<SymbolsScopeEntry>(this, nameof(Symbol));
        }

        public override int CountChildren() => 0;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            // No-Op
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return this;
        }

        public override void Analyze(SemanticContext context)
        {
            var identifier = GetSyntaxNode<AST.Identifier>();

            if (identifier.IsExpression)
            {
                var symbol = Scope.TryLookup(identifier.Name);

                if (symbol != null)
                {
                    Type.Resolve(context, symbol.Type);
                    Symbol.Resolve(context, symbol);
                }
                else
                {
                    context.RegisterDependency(new SymbolDependency(Scope, identifier.Name));
                }
            }
        }
    }
}
