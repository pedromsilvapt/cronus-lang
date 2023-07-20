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
    public abstract class Expression : Node
    {
        #region Semantic Properties

        public SemanticProperty<TypeDefinition> Type { get; set; }

        #endregion
        
        public Expression(SymbolsScope scope, AST.Node syntaxNode) : base(scope, syntaxNode)
        {
            Type = new SemanticProperty<TypeDefinition>(this, nameof(Type));
        }
    }
}
