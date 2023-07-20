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

        public StackFrame CurrentFrame { get; set; }

        public CronusVM(ByteCode.ByteCode byteCode)
        {
            ByteCode = byteCode;
            Headers = new List<HeaderStruct>();
            Functions = new Dictionary<int, FunctionStruct>();
            FunctionsBySymbol = new Dictionary<string, FunctionStruct>();
            Stack = new ByteStream();
        }

        public void Execute()
        {
            ReadHeaders();

            InitializeGlobalSymbols();

            Console.WriteLine(Stack.ReadInt());
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

                    Functions[function.TypeId] = function;
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

                    Stack.Write(Stack.PopInt() + Stack.PopInt());

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

                    Stack.Write(Stack.PopInt() * Stack.PopInt());

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
                    Stack.Write(CurrentFrame.StackPointer);
                }
                else if (opcode == OpCode.PushFP)
                {
                    Stack.Write(CurrentFrame.FramePointer);
                }
                else if (opcode == OpCode.PushIP)
                {
                    Stack.Write(ByteCode.Cursor);
                }
                else if (opcode == OpCode.LoadStack)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(CurrentFrame.StackPointer + offset));
                }
                else if (opcode == OpCode.LoadStackN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    Stack.Write(ReadStack(CurrentFrame.StackPointer + offset, size));
                }
                else if (opcode == OpCode.LoadStackDyn)
                {
                    int offset = Stack.ReadInt();

                    int size = Stack.ReadInt();

                    if (size == 1)
                    {
                        Stack.Write(ReadStack(CurrentFrame.StackPointer + offset));
                    }
                    else if (size > 0)
                    {
                        Stack.Write(ReadStack(CurrentFrame.StackPointer + offset, size));
                    }
                }
                else if (opcode == OpCode.LoadFrame)
                {
                    int offset = ByteCode.ReadInt();

                    Stack.Write(ReadStack(CurrentFrame.FramePointer + offset));
                }
                else if (opcode == OpCode.LoadFrameN)
                {
                    int offset = ByteCode.ReadInt();

                    int size = ByteCode.ReadInt();

                    Stack.Write(ReadStack(CurrentFrame.FramePointer + offset, size));
                }
                else if (opcode == OpCode.LoadFrameDyn)
                {
                    int offset = Stack.ReadInt();

                    int size = Stack.ReadInt();

                    if (size == 1)
                    {
                        Stack.Write(ReadStack(CurrentFrame.FramePointer + offset));
                    }
                    else if (size > 0)
                    {
                        Stack.Write(ReadStack(CurrentFrame.FramePointer + offset, size));
                    }
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
    }

    public struct StackFrame
    {
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