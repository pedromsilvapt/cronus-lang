using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.ByteCode
{
    public enum OpCode
    {
        // Type Definitions
        DefFunc,
        //DefStruct,
        //DefEnum,
        //DefTuple,
        // Bindings
        Let,
        Return,
        Call,
        Halt,

        // Control Flow
        Jump,
        JumpCond,

        // Generic Operators
        Eq,
        Neq,

        // Integer
        PushInt,
        AddInt,
        SubInt,
        MulInt,
        DivInt,
        NegInt,
        PowInt,

        LtInt,
        LteInt,
        GtInt,
        GteInt,

        // Decimals
        PushDec,
        AddDec,
        SubDec,
        MulDec,
        DivDec,
        NegDec,
        PowDec,

        LtDec,
        LteDec,
        GtDec,
        GteDec,

        // Bool
        PushTrue,
        PushFalse,
        Not,

        // Conversion
        IntToDec,
        DecToInt,
        IntToBool,
        BoolToInt,

        #region Push Pointers

        /// <summary>
        /// Pushes the value of the Stack Pointer onto the Stack
        /// </summary>
        PushSP,

        /// <summary>
        /// Pushes the value of the Frame Pointer onto the Stack
        /// </summary>
        PushFP,

        /// <summary>
        /// Pushes the value of the Instruction Pointer onto the Stack
        /// </summary>
        PushIP,

        #endregion

        #region Generic Pushes

        /// <summary>
        /// Pushes N bytes (zeroed) into the stack
        /// </summary>
        PushN,

        #endregion

        #region Pops

        Pop,

        PopN,

        PopDyn,

        #endregion

        #region Loads

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Stack Pointer
        /// 
        /// LoadStack X <=> Push(StackPointer + X)
        /// </summary>
        LoadStack,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Stack Pointer
        /// 
        /// LoadStackN X N <=> for (I in 0..N) Push(StackPointer + X + I)
        /// </summary>
        LoadStackN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Stack Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadStackDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(StackPointer + X + I)
        /// </summary>
        LoadStackDyn,

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Frame Pointer
        /// 
        /// LoadFrame X <=> Push(FramePointer + X)
        /// </summary>
        LoadFrame,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// 
        /// LoadFrame X N <=> for (I in 0..N) Push(FramePointer + X + I)
        /// </summary>
        LoadFrameN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadFrameDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(FramePointer + X + I)
        /// </summary>
        LoadFrameDyn,

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Frame Pointer
        /// 
        /// LoadGlobal X <=> Push(GlobalPointer + X)
        /// </summary>
        LoadGlobal,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// 
        /// LoadGlobalN X N <=> for (I in 0..N) Push(GlobalPointer + X + I)
        /// </summary>
        LoadGlobalN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadGlobalDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(GlobalPointer + X + I)
        /// </summary>
        LoadGlobalDyn,

        #endregion

        #region Stores

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Stack Pointer
        /// 
        /// StoreStack X <=> Store(StackPointer + X)
        /// </summary>
        StoreStack,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Stack Pointer
        /// 
        /// StoreStackN X N <=> for (I in 0..N) Push(StackPointer + X + I)
        /// </summary>
        StoreStackN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Stack Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadStackDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(StackPointer + X + I)
        /// </summary>
        StoreStackDyn,

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Frame Pointer
        /// 
        /// LoadFrame X <=> Push(FramePointer + X)
        /// </summary>
        StoreFrame,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// 
        /// LoadFrame X N <=> for (I in 0..N) Push(FramePointer + X + I)
        /// </summary>
        StoreFrameN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadFrameDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(FramePointer + X + I)
        /// </summary>
        StoreFrameDyn,

        /// <summary>
        /// Pushes onto the top of the stack the byte relative to the Frame Pointer
        /// 
        /// LoadGlobal X <=> Push(GlobalPointer + X)
        /// </summary>
        StoreGlobal,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// 
        /// LoadGlobalN X N <=> for (I in 0..N) Push(GlobalPointer + X + I)
        /// </summary>
        StoreGlobalN,

        /// <summary>
        /// Pushes onto the top of the stack the N bytes relative to the Frame Pointer
        /// The offset and the number of bytes are stored on the stack.
        /// 
        /// LoadGlobalDyn <=> X = Pop; N = Pop; for (I in 0..N) Push(GlobalPointer + X + I)
        /// </summary>
        StoreGlobalDyn,

        #endregion

        //Pop = 4,
        //PopN = 5,
        //Dup = 6,
        //DupN = 7,
    }
}
