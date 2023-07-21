using CronusLang.ByteCode;

namespace CronusLang
{
    public class CronusVM
    {
        public ByteCode.ByteCode ByteCode { get; set; }

        public List<HeaderStruct> Headers { get; set; }

        public Dictionary<int, FunctionStruct> Functions { get; set; }

        public Dictionary<string, FunctionStruct> FunctionsBySymbol { get; set; }

        public ByteStream Stack { get; set; }

        public Registers Registers { get; set; }

        public CronusVM(ByteCode.ByteCode byteCode)
        {
            ByteCode = byteCode;
            Headers = new List<HeaderStruct>();
            Functions = new Dictionary<int, FunctionStruct>();
            FunctionsBySymbol = new Dictionary<string, FunctionStruct>();
            Stack = new ByteStream();
            Registers = new Registers(this);
        }

        public void Execute()
        {
            ReadHeaders();

            InitializeGlobalSymbols();
        }

        protected void ReadHeaders()
        {
            Headers.Clear();
            Functions.Clear();
            FunctionsBySymbol.Clear();

            ByteCode.Cursor = 0;

            while (!ByteCode.EOF)
            {
                var header = ByteCode.ReadHeader();

                Headers.Add(header);

                ByteCode.Cursor = header.FunctionsIndex;

                int functionsCount = ByteCode.ReadInt();
                for (int i = 0; i < functionsCount; i++)
                {
                    var function = ByteCode.ReadFunction();

                    Functions[function.FunctionId] = function;
                    FunctionsBySymbol[function.Symbol] = function;

                    // TODO Associate the functions with the header they belong to
                }

                // Move to the end of the section covered by this header. If we have more headers, we can continue reading them
                // This way we can easily concatenate libraries (TODO not really supported now because of absolute jump positions)
                ByteCode.Cursor = header.HeaderIndex + header.TotalLength;
            }
        }

        protected void InitializeGlobalSymbols()
        {
            foreach (var header in Headers)
            {
                ByteCode.Cursor = header.CodeStartIndex;

                ExecuteInstructions();
            }

            ByteCode.Cursor = 0;
        }

        protected void ExecuteInstructions()
        {

            while (!ByteCode.EOF)
            {
                // Always guaranteed to not fail because OpCodes take only a single byte, and we are not yet at the EOF
                var opcode = ByteCode.ReadOpCode();

                if (opcode == OpCode.PushInt)
                {
                    #region PushInt

                    Stack.Write(ByteCode.ReadInt());

                    #endregion
                }
                else if (opcode == OpCode.AddInt)
                {
                    #region AddInt

                    int result = Stack.PopInt() + Stack.PopInt();
                    Stack.Write(result);

                    #endregion
                }
                else if (opcode == OpCode.SubInt)
                {
                    #region SubInt

                    var op2 = Stack.PopInt();
                    var op1 = Stack.PopInt();
                    Stack.Write(op1 - op2);

                    #endregion
                }
                else if (opcode == OpCode.MulInt)
                {
                    #region SubInt

                    var result = Stack.PopInt() * Stack.PopInt();
                    Stack.Write(result);

                    #endregion
                }
                else if (opcode == OpCode.DivInt)
                {
                    #region DivInt

                    var op2 = Stack.PopInt();
                    var op1 = Stack.PopInt();
                    Stack.Write(op1 / op2);

                    #endregion
                }
                else if (opcode == OpCode.PushSP)
                {
                    Stack.Write(Registers.StackPointer);
                }
                else if (opcode == OpCode.PushFP)
                {
                    Stack.Write(Registers.FramePointer);
                }
                else if (opcode == OpCode.PushIP)
                {
                    Stack.Write(ByteCode.Cursor);
                }
                else if (opcode == OpCode.LoadStack)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.StackPointer + offset));
                }
                else if (opcode == OpCode.LoadStackN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.StackPointer + offset, size));
                }
                else if (opcode == OpCode.LoadStackDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        Stack.Write(ReadStack(Registers.StackPointer + offset));
                    }
                    else if (size > 0)
                    {
                        Stack.Write(ReadStack(Registers.StackPointer + offset, size));
                    }
                }
                else if (opcode == OpCode.LoadFrame)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.FramePointer + offset));
                }
                else if (opcode == OpCode.LoadFrameN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.FramePointer + offset, size));
                }
                else if (opcode == OpCode.LoadFrameDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        Stack.Write(ReadStack(Registers.FramePointer + offset));
                    }
                    else if (size > 0)
                    {
                        Stack.Write(ReadStack(Registers.FramePointer + offset, size));
                    }
                }
                else if (opcode == OpCode.LoadGlobal)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.GlobalPointer + offset));
                }
                else if (opcode == OpCode.LoadGlobalN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.GlobalPointer + offset, size));
                }
                else if (opcode == OpCode.LoadGlobalDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        Stack.Write(ReadStack(Registers.GlobalPointer + offset));
                    }
                    else if (size > 0)
                    {
                        Stack.Write(ReadStack(Registers.FramePointer + offset, size));
                    }
                }
                else if (opcode == OpCode.StoreStack)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.StackPointer + offset));
                }
                else if (opcode == OpCode.StoreStackN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    WriteStack(Registers.StackPointer + offset, Stack.Pop(size));
                }
                else if (opcode == OpCode.StoreStackDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        WriteStack(Registers.StackPointer + offset, Stack.Pop());
                    }
                    else if (size > 0)
                    {
                        WriteStack(Registers.StackPointer + offset, Stack.Pop(size));
                    }
                }
                else if (opcode == OpCode.StoreFrame)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.FramePointer + offset));
                }
                else if (opcode == OpCode.StoreFrameN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    WriteStack(Registers.FramePointer + offset, Stack.Pop(size));
                }
                else if (opcode == OpCode.StoreFrameDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        WriteStack(Registers.FramePointer + offset, Stack.Pop());
                    }
                    else if (size > 0)
                    {
                        WriteStack(Registers.FramePointer + offset, Stack.Pop(size));
                    }
                }
                else if (opcode == OpCode.StoreGlobal)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(Registers.GlobalPointer + offset));
                }
                else if (opcode == OpCode.StoreGlobalN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    WriteStack(Registers.GlobalPointer + offset, Stack.Pop(size));
                }
                else if (opcode == OpCode.StoreGlobalDyn)
                {
                    int size = Stack.PopInt();

                    int offset = Stack.PopInt();

                    if (size == 1)
                    {
                        WriteStack(Registers.GlobalPointer + offset, Stack.Pop());
                    }
                    else if (size > 0)
                    {
                        WriteStack(Registers.GlobalPointer + offset, Stack.Pop(size));
                    }
                }
                else if (opcode == OpCode.PushN)
                {
                    int size = ByteCode.ReadInt();

                    if (size == 1)
                    {
                        Stack.Write((byte)0);
                    }
                    else if (size > 0)
                    {
                        Stack.Write(new byte[size]);
                    }
                }
                else if (opcode == OpCode.PopN)
                {
                    var size = ByteCode.ReadInt();

                    Registers.StackPointer -= size;
                }
                else if (opcode == OpCode.Call)
                {
                    int contextPointer = Stack.PopInt();

                    int functionId = Stack.PopInt();

                    if (Functions.ContainsKey(functionId))
                    {
                        var fn = Functions[functionId];

                        var registers = Registers.Snapshot();

                        Stack.Write(registers.InstructionPointer);
                        Stack.Write(registers.FramePointer);

                        Registers.FramePointer = Registers.StackPointer;
                        Registers.InstructionPointer = fn.Position;
                    }
                    else
                    {
                        throw new Exception("No function registered with id " + functionId);
                    }
                }
                else if (opcode == OpCode.Return)
                {
                    Registers.StackPointer = Registers.FramePointer;
                    
                    Registers.FramePointer = Stack.PopInt();
                    Registers.InstructionPointer = Stack.PopInt();
                }
                else if (opcode == OpCode.LteInt)
                {
                    var op2 = Stack.PopInt();

                    var op1 = Stack.PopInt();

                    Stack.Write(op1 <= op2);
                }
                else if (opcode == OpCode.Jump)
                {
                    var jumpAddress = ByteCode.ReadInt();

                    Registers.InstructionPointer = jumpAddress;
                }
                else if (opcode == OpCode.JumpCond)
                {
                    var jumpAddress = ByteCode.ReadInt();

                    var cond = Stack.PopBool();

                    if (!cond)
                    {
                        Registers.InstructionPointer = jumpAddress;
                    }
                }
                else if (opcode == OpCode.Nop)
                {
                    // No-op
                }
                else if (opcode == OpCode.Halt)
                {
                    break;
                }
                else
                {
                    throw new Exception($"Not yet implemented opcode {opcode}");
                }
            }
        }

        protected byte ReadStack(int offset)
        {
            var restore = Stack.Cursor;
            Stack.Cursor = offset;
            var value = Stack.Read();
            Stack.Cursor = restore;
            return value;
        }

        protected byte[] ReadStack(int offset, int size)
        {
            var restore = Stack.Cursor;
            Stack.Cursor = offset;
            var value = Stack.Read(size);
            Stack.Cursor = restore;
            return value;
        }

        protected void WriteStack(int offset, byte value)
        {
            var restore = Stack.Cursor;
            Stack.Cursor = offset;
            Stack.Write(value);
            Stack.Cursor = restore;
        }

        protected void WriteStack(int offset, byte[] value)
        {
            var restore = Stack.Cursor;
            Stack.Cursor = offset;
            Stack.Write(value);
            Stack.Cursor = restore;
        }
    }

    public class Registers
    {
        protected CronusVM _vm;

        public Registers(CronusVM vm)
        {
            this._vm = vm;
        }

        /// <summary>
        /// Global pointer
        /// </summary>
        public int GlobalPointer { get; set; }

        /// <summary>
        /// The position of the top (end) of the current stack frame
        /// </summary>
        public int StackPointer
        {
            get => _vm.Stack.Cursor;
            set => _vm.Stack.Cursor = value;
        }

        /// <summary>
        /// The position of the bottom (start) of the current stack frame
        /// It's also the position of the top (end) of the previous stack frame
        /// </summary>
        public int FramePointer { get; set; }

        /// <summary>
        /// The position of the instruction pointer indicates the next instruction to be read
        /// </summary>
        public int InstructionPointer
        {
            get => _vm.ByteCode.Cursor;
            set => _vm.ByteCode.Cursor = value;
        }

        public RegistersSnapshot Snapshot()
        {
            return new RegistersSnapshot
            {
                GlobalPointer = GlobalPointer,
                FramePointer = FramePointer,
                InstructionPointer = InstructionPointer,
                StackPointer = StackPointer,
            };
        }
    }

    public struct RegistersSnapshot
    {
        /// <summary>
        /// Global pointer
        /// </summary>
        public int GlobalPointer;

        /// <summary>
        /// The position of the top (end) of the current stack frame
        /// </summary>
        public int StackPointer;

        /// <summary>
        /// The position of the bottom (start) of the current stack frame
        /// It's also the position of the top (end) of the previous stack frame
        /// </summary>
        public int FramePointer;

        /// <summary>
        /// The position of the instruction pointer indicates the next instruction to be read
        /// </summary>
        public int InstructionPointer;
    }
}