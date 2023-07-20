using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Literals
{
    public class StringLiteral : Literal<string>
    {
        public StringLiteral(SymbolsScope scope, AST.Literals.StringLiteral syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            var typeDef = context.StringType;

            Type.Resolve(context, typeDef);
        }
    }
}
