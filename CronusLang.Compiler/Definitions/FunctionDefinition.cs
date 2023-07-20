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

        public FunctionDefinition(int id, Symbol symbol, FunctionTypeDefinition type, string[] argNames, string[] bindingNames) : base()
        {
            Id = id;
            Symbol = symbol;
            Type = type;
            Instructions = new List<Instruction>();
            ArgNames = argNames;
            BindingNames = bindingNames;
            Variables = GetArgumentVariablePositions(type, argNames);
        }

        public void Emit(OpCode opcode, params object[] args)
        {
            Emit(new Instruction(opcode, args));
        }

        public void Emit(Instruction instruction)
        {
            Instructions.Add(instruction);
        }

        /// <summary>
        /// Build the Dictionary of variables and their byte offset positions relative to the Frame Pointer.
        /// 
        /// Sample of a stack where each row is one byte. LocalVar1 and LocalVar3 have 1 byte of size, while LocalVar2 has 2 bytes.
        /// Similarly, Arg1, Arg2 and Arg3 have 1 byte of size as well. The return value, has 3 bytes.
        /// 
        /// ┌────────────────┐
        /// │        .       │
        /// │        .       │
        /// │        .       │
        /// │    LocalVar3   │+3
        /// │                │
        /// │    LocalVar2   │+1
        /// │    LocalVar1   │+0
        /// ├────────────────┤FP
        /// │      Arg3      │-1
        /// │      Arg2      │-2
        /// │      Arg1      │-3
        /// │                │
        /// │                │
        /// │     Return     │-6
        /// └────────────────┘
        /// </summary>
        /// <param name="type"></param>
        /// <param name="argNames"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, SymbolDefinition> GetArgumentVariablePositions(FunctionTypeDefinition type, string[] argNames)
        {
            if (argNames.Length != type.Arguments.Count)
            {
                throw new Exception("Invalid number of argument names: does not match function's type argument's number");
            }

            var variables = new Dictionary<string, SymbolDefinition>();

            // Leave space for the frame pointer and stack pointer values
            int offset = sizeof(int) * -2;

            for (int i = argNames.Length; i > 0; i--)
            {
                // Get the size of the argument
                offset -= type.Arguments[i - 1].Type.GetSize();

                variables[argNames[i - 1]] = new SymbolDefinition(isGlobal: false, OpCode.LoadFrameN, OpCode.StoreFrameN, offset, type.Arguments[i - 1].Type);
            }

            offset -= type.ReturnType.GetSize();

            variables["@return"] = new SymbolDefinition(isGlobal: false, OpCode.LoadFrameN, OpCode.StoreFrameN, offset, type.ReturnType);

            // TODO Create @captures variable
            // TODO Create @framePointer variables

            return variables;
        }
    }
}
