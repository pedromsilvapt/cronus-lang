using CronusLang.Compiler.Definitions;
using CronusLang.TypeSystem;
using CronusLang.TypeSystem.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Containers
{
    public class FunctionsContainer
    {
        protected int _functionIdCounter = 1;

        public Dictionary<SymbolIdentifier, FunctionDefinition> FunctionsBySymbol { get; set; } = new Dictionary<SymbolIdentifier, FunctionDefinition>();

        public Dictionary<int, FunctionDefinition> FunctionsById { get; set; } = new Dictionary<int, FunctionDefinition>();

        /// <summary>
        /// Create a function definition. Instructions can then be added to the function definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FunctionDefinition CreateFunction(SymbolIdentifier symbol, FunctionTypeDefinition type, string[] argNames, Dictionary<Symbol, SymbolDefinition> variables)
        {
            var id = _functionIdCounter++;

            var definition = new FunctionDefinition(id, symbol, type, argNames, variables);

            FunctionsById[id] = definition;

            FunctionsBySymbol[symbol] = definition;

            return definition;
        }

    }
}
