using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators
{
    public abstract class BinaryOperator : Expression
    {
        public Expression Left { get; set; }

        public Expression Right { get; set; }

        public BinaryOperator(SymbolsScope scope, AST.Operators.BinaryOperator syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Left = transformer.ToSST<SST.Expression>(scope, syntaxNode.Left);
            Right = transformer.ToSST<SST.Expression>(scope, syntaxNode.Right);
        }

        public override int CountChildren() => 2;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Left;
            childrenReceiver[1] = Right;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }
    }
}
