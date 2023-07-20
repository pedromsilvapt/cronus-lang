using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Expressions
{
    public class DynAccessor : Node
    {
        public Expression Container { get; protected set; }

        public Expression Member { get; protected set; }

        public DynAccessor(SymbolsScope scope, AST.Expressions.DynAccessor syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Container = transformer.ToSST<SST.Expression>(scope, syntaxNode.Container);
            Member = transformer.ToSST<SST.Expression>(scope, syntaxNode.Member);
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
