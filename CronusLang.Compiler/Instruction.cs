using CronusLang.ByteCode;
using CronusLang.TypeSystem;

namespace CronusLang.Compiler
{
    public class Instruction
    {
        public OpCode OpCode { get; set; }

        public object[] Args { get; set; }

        public Instruction(OpCode opcode, params object[] args)
        {
            OpCode = opcode;
            Args = args;
        }
    }

    public struct LabelPlaceholder
    {
        public int LabelId;

        public LabelPlaceholder(int labelId)
        {
            LabelId = labelId;
        }
    }

    public struct FunctionPlaceholder {
        public Symbol Symbol;

        public FunctionPlaceholder(Symbol symbol)
        {
            Symbol = symbol;
        }
    }

    public struct VariablePlaceholder
    {
        public Symbol Symbol;

        public VariablePlaceholder(Symbol symbol)
        {
            Symbol = symbol;
        }
    }
}
