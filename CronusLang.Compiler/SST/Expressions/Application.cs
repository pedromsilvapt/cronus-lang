using CronusLang.TypeSystem.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Expressions
{
    public class Application : Expression
    {
        public SST.Expression Func { get; protected set; }

        public IList<SST.Expression> Args { get; protected set; }

        public Application(SymbolsScope scope, AST.Expressions.Application syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
            Func = transformer.ToSST<SST.Expression>(scope, syntaxNode.Func);
            Args = transformer.ToSST<SST.Expression>(scope, syntaxNode.Args);
        }

        public override int CountChildren() => 1 + Args.Count;

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            childrenReceiver[0] = Func;
            for (int i = 0; i < Args.Count; i++) childrenReceiver[i + 1] = Args[i];
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            throw new NotImplementedException();
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                if (Func.Type.TrackDependency(context) &&
                    Args.All(arg => arg.Type.TrackDependency(context)))
                {
                    // TODO Type check arguments
                    var functionType = (FunctionTypeDefinition)Func.Type.Value;

                    Type.Resolve(context, functionType.ReturnType);
                }
            }
        }
    }
}
