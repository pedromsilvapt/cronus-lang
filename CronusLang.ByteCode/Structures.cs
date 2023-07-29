using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.ByteCode
{
    public struct HeaderStruct
    {
        public int HeaderIndex;

        public RangeStruct InstructionsRange;

        public int FunctionsIndex;

        public int TypesIndex;

        public int SymbolsIndex;

        public RangeStruct SourceRange;

        public RangeStruct SourceMapsRange;

        public int TotalLength;
    }

    public struct FunctionStruct
    {
        public int FunctionId;

        public int Position;

        public int TypeId;

        public string[] ArgNames;

        public string Symbol;
    }

    public struct RangeStruct
    {
        public int Start;

        public int End;

        public bool Empty => End == Start;
    }

    #region Types

    public struct TypeStruct
    {
        public int TypeId;

        public bool IsBool;

        public bool IsInt;

        public bool IsDecimal;

        public FunctionTypeStruct? Function;
    }

    public struct FunctionTypeStruct
    {
        public FunctionArgumentTypeStruct[] Arguments;

        public int ReturnType;
    }

    public struct FunctionArgumentTypeStruct
    {
        public string? Name;

        public int Type;
    }

    #endregion

    #region Symbols

    public struct SymbolStruct
    {
        public int SymbolId;

        public int TypeId;

        public int? FunctionId;

        public string? Name;
    }

    #endregion

    #region Source Map

    public struct LocationStruct
    {
        public RangeStruct Instructions;

        public RangeStruct Source;
    }
    
    #endregion
}
