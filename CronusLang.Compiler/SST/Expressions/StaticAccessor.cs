using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Expressions
{
    public class StaticAccessor : Node
    {
        public Expression Container { get; protected set; }

        public Identifier Member { get; protected set; }

        public StaticAccessor(SymbolsScope scope, AST.Expressions.StaticAccessor syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Container = transformer.ToSST<SST.Expression>(scope, syntaxNode.Container);
            Member = transformer.ToSST<SST.Identifier>(scope, syntaxNode.Member);
        }

        public override int CountChildren() => 2;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Container;
            childrenReceiver[1] = Member;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            throw new NotImplementedException();
        }
    }
}
