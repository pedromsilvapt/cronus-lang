using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Literals
{
    public abstract class Literal<T> : Expression
    {
        public Literal(SymbolsScope scope, AST.Literals.Literal<T> syntaxNode) : base(scope, syntaxNode)
        {
        }

        public override int CountChildren() => 0;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            // No-op
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            return this;
        }
    }
}
