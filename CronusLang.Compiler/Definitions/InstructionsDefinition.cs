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

        public List<Instruction> Instructions { get; set; }

        public Dictionary<string, SymbolDefinition> Variables { get; set; }

        public string[] BindingNames { get; set; }

        public Dictionary<int, int> Labels { get; set; }

        public int StackPointerOffset { get; protected set; } = 0;

        public virtual OpCode LoadOperation { get; set; } = OpCode.LoadFrameN;

        public virtual OpCode StoreOperation { get; set; } = OpCode.StoreFrameN;

        public InstructionsDefinition()
        {
            Instructions = new List<Instruction>();
            Variables = new Dictionary<string, SymbolDefinition>();
            BindingNames = new string[0];
            Labels = new Dictionary<int, int>();
        }

        public InstructionsDefinition(InstructionsDefinition parent)
        {
            Instructions = new List<Instruction>();
            Variables = parent.Variables.ToDictionary(kv => kv.Key, kv => kv.Value);
            BindingNames = parent.BindingNames.ToArray();
            Labels = new Dictionary<int, int>();
            StackPointerOffset = parent.StackPointerOffset;
        }

        public SymbolDefinition CreateVariable(string variableName, TypeDefinition variableType)
        {
            if (Variables.ContainsKey(variableName))
            {
                throw new Exception($"Cannot redeclare variables (variable '{variableName}' already exists)");
            }

            int offset = StackPointerOffset;

            var symbol = new SymbolDefinition(IsGlobal, LoadOperation, StoreOperation, offset, variableType);

            Variables.Add(variableName, symbol);

            StackPointerOffset += variableType.GetSize();

            return symbol;
        }

        public SymbolDefinition GetVariable(string variableName)
        {
            if (!Variables.ContainsKey(variableName))
            {
                throw new Exception($"Variable named {variableName} not defined in the scope.");
            }

            return Variables[variableName];
        }

        public int GetVariableOffset(string variableName)
        {
            return GetVariable(variableName).Index;
        }

    }
}
