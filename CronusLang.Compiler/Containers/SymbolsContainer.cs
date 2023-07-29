using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Containers
{
    public class SymbolsContainer
    {
        protected int _labelIdCounter = 1;

        public Dictionary<int, Symbol> SymbolsById { get; set; } = new Dictionary<int, Symbol>();

        public void Register(string identifier, Symbol symbol)
        {
            SymbolsById[symbol.SymbolId] = symbol;
        }

        public Symbol CreateCapture(SymbolsScope sourceScope, string identifier, TypeDefinition type)
        {
            return Symbol.CreateCapture(_labelIdCounter++, sourceScope, identifier, type);
        }

        public Symbol CreateParameter(int parameterIndex, TypeDefinition type)
        {
            return Symbol.CreateParameter(_labelIdCounter++, parameterIndex, type);
        }

        public Symbol CreateBinding(int bindingIndex, bool global, TypeDefinition type)
        {
            return Symbol.CreateBinding(_labelIdCounter++, bindingIndex, global, type);
        }

        public Symbol CreateType(TypeDefinition type)
        {
            return Symbol.CreateType(_labelIdCounter++, type);
        }
    }
}
