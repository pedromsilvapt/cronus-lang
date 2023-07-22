using CronusLang.ByteCode;
using CronusLang.TypeSystem;
using CronusLang.TypeSystem.Types;

namespace CronusLang.Compiler.Definitions
{
    public class FunctionDefinition : InstructionsDefinition
    {
        public int Id { get; set; }

        public Symbol Symbol { get; set; }

        public FunctionTypeDefinition Type { get; set; }

        public string[] ArgNames { get; set; }

        public FunctionDefinition(int id, Symbol symbol, FunctionTypeDefinition type, string[] argNames, Dictionary<SymbolsScopeEntry, SymbolDefinition> variables) : base()
        {
            Id = id;
            Symbol = symbol;
            Type = type;
            Instructions = new List<Instruction>();
            ArgNames = argNames;
            Variables = variables;
        }

        public void Emit(OpCode opcode, params object[] args)
        {
            Emit(new Instruction(opcode, args));
        }

        public void Emit(Instruction instruction)
        {
            Instructions.Add(instruction);
        }
    }
}
