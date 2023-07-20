using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators
{
    public abstract class UnaryOperator : Expression
    {
        public Expression Right { get; set; }

        public UnaryOperator(SymbolsScope scope, AST.Operators.UnaryOperator syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Right = transformer.ToSST<SST.Expression>(scope, syntaxNode.Right);
        }

        public override int CountChildren() => 1;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Right;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }
    }
}
