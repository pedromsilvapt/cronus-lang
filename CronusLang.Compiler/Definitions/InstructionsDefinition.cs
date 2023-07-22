using CronusLang.ByteCode;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Definitions
{
    public class InstructionsDefinition
    {
        public virtual bool IsGlobal => false;

        public virtual SymbolsScope RootScope => null;

        public List<Instruction> Instructions { get; set; }

        public Dictionary<SymbolsScopeEntry, SymbolDefinition> Variables { get; set; }

        public Dictionary<string, SymbolsScopeEntry> VariableNames { get; set; }

        public Dictionary<int, int> Labels { get; set; }

        public int StackPointerOffset { get; set; } = 0;

        public virtual OpCode LoadOperation { get; set; } = OpCode.LoadFrameN;

        public virtual OpCode StoreOperation { get; set; } = OpCode.StoreFrameN;

        public InstructionsDefinition()
        {
            Instructions = new List<Instruction>();
            Variables = new Dictionary<SymbolsScopeEntry, SymbolDefinition>();
            VariableNames = new Dictionary<string, SymbolsScopeEntry>();
            Labels = new Dictionary<int, int>();
        }

        [Obsolete]
        public InstructionsDefinition(InstructionsDefinition parent)
        {
            Instructions = new List<Instruction>();
            Variables = parent.Variables.ToDictionary(kv => kv.Key, kv => kv.Value);
            VariableNames = parent.VariableNames.ToDictionary(kv => kv.Key, kv => kv.Value);
            Labels = new Dictionary<int, int>();
            StackPointerOffset = parent.StackPointerOffset;
        }

        public SymbolDefinition CreateVariable(string variableName, TypeDefinition variableType, int offset)
        {
            if (VariableNames.ContainsKey(variableName))
            {
                throw new Exception($"Cannot redeclare variables (variable '{variableName}' already exists)");
            }

            var scopeSymbol = new SymbolsScopeEntry();

            var symbol = new SymbolDefinition(IsGlobal, LoadOperation, StoreOperation, offset, variableType);

            Variables.Add(scopeSymbol, symbol);
            VariableNames.Add(variableName, scopeSymbol);

            return symbol;
        }

        public SymbolDefinition GetVariable(string variableName)
        {
            if (!VariableNames.ContainsKey(variableName))
            {
                throw new Exception($"Variable named {variableName} not defined in the scope.");
            }

            return Variables[VariableNames[variableName]];
        }

        public SymbolDefinition GetVariable(SymbolsScopeEntry scopeSymbol)
        {
            // TODO Give unique global Ids to SymbolsScopeEntry, and register symbol information
            // associated with those Ids in the bytecode (just like functions)
            // Possible metadata is the symbol name and location, for example
            if (!Variables.ContainsKey(scopeSymbol))
            {
                throw new Exception($"Variable named {scopeSymbol} not defined in the scope.");
            }

            return Variables[scopeSymbol];
        }
    }
}
