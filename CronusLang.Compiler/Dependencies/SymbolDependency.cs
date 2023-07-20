using CronusLang.Compiler.SST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.Dependencies
{
    public class SymbolDependency : SemanticDependency
    {
        public SymbolsScope Scope { get; protected set; }

        public string Identifier { get; protected set; }

        public SymbolDependency(SymbolsScope scope, string identifier) : base()
        {
            Scope = scope;
            Identifier = identifier;
        }

        public override bool Equals(object? obj)
        {
            SymbolDependency? dependency = obj as SymbolDependency;

            if (dependency == null)
            {
                return false;
            }

            return dependency.Scope == Scope && dependency.Identifier == Identifier;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + GetType().GetHashCode();
            hash = (hash * 7) + Scope.GetHashCode();
            hash = (hash * 7) + Identifier.GetHashCode();
            return hash;
        }

        public override DiagnosticMessage? GetUnresolvedDiagnostic(Node dependant)
        {
            return new DiagnosticMessage(DiagnosticLevel.Error, $"Unresolved symbol {Identifier} in scope {Scope.FullName}", dependant.SyntaxNode.Location);
        }

        public override string ToString()
        {
            return $"SymbolDependency({Scope.FullName}, {Identifier})";
        }
    }
}
