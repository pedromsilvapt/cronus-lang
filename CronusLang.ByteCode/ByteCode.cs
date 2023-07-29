using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.ByteCode
{
    public class ByteCode : ByteStream
    {
        public int Write(HeaderStruct header)
        {
            int cursor = Cursor;

            Write(header.HeaderIndex);
            Write(header.TypesIndex);
            Write(header.FunctionsIndex);
            Write(header.InstructionsRange.Start);
            Write(header.InstructionsRange.End);
            Write(header.SourceRange.Start);
            Write(header.SourceRange.End);
            Write(header.TotalLength);
            
            return cursor;
        }

        public int Write(FunctionStruct function)
        {
            int cursor = Cursor;

            Write(function.FunctionId);
            Write(function.Position);
            Write(function.TypeId);
            Write(function.ArgNames.Count());
            foreach (var arg in function.ArgNames) Write(arg);
            Write(function.Symbol);

            return cursor;
        }

        public int Write(TypeStruct type)
        {
            int cursor = Cursor;

            Write(type.TypeId);
            Write(type.IsBool);
            Write(type.IsInt);
            Write(type.IsDecimal);
            Write(type.Function.HasValue);
            if (type.Function.HasValue)
            {
                Write(type.Function.Value);
            }

            return cursor;
        }

        public int Write(FunctionTypeStruct functionType)
        {
            int cursor = Cursor;

            Write(functionType.Arguments.Length);
            foreach (var arg in functionType.Arguments)
            {
                Write(arg);
            }
            Write(functionType.ReturnType);

            return cursor;
        }

        public int Write(FunctionArgumentTypeStruct functionArgumentType)
        {
            int cursor = Cursor;

            Write(functionArgumentType.Name);
            Write(functionArgumentType.Type);

            return cursor;
        }

        public int Write(SymbolStruct symbol)
        {
            int cursor = Cursor;

            Write(symbol.SymbolId);
            Write(symbol.TypeId);
            Write(symbol.FunctionId ?? 0);
            Write(symbol.Name);
            
            return cursor;
        }

        public int Write(LocationStruct location)
        {
            int cursor = Cursor;

            Write(location.Instructions.Start);
            Write(location.Instructions.End);
            Write(location.Source.Start);
            Write(location.Source.End);
            
            return cursor;
        }

        #region Read

        public HeaderStruct ReadHeader()
        {
            var header = new HeaderStruct();
            header.HeaderIndex = ReadInt();
            header.TypesIndex = ReadInt();
            header.FunctionsIndex = ReadInt();
            header.InstructionsRange.Start = ReadInt();
            header.InstructionsRange.End = ReadInt();
            header.SourceRange.Start = ReadInt();
            header.SourceRange.End = ReadInt();
            header.TotalLength = ReadInt();
            return header;
        }

        public FunctionStruct ReadFunction()
        {
            var function = new FunctionStruct();

            function.FunctionId = ReadInt();
            function.Position = ReadInt();
            function.TypeId = ReadInt();
            var argsCount = ReadInt();
            function.ArgNames = Enumerable.Range(0, argsCount).Select(_ => ReadString() ?? string.Empty).ToArray();
            function.Symbol = ReadString() ?? string.Empty;

            return function;
        }

        public TypeStruct ReadType()
        {
            var type = new TypeStruct();

            type.TypeId = ReadInt();
            type.IsBool = ReadBool();
            type.IsInt = ReadBool();
            type.IsDecimal = ReadBool();
            type.Function = ReadBool()
                ? ReadFunctionType()
                : null;
            
            return type;
        }

        public FunctionTypeStruct ReadFunctionType()
        {
            var func = new FunctionTypeStruct();

            func.Arguments = new FunctionArgumentTypeStruct[ReadInt()];
            for (int i = 0; i < func.Arguments.Length; i++)
            {
                var arg = new FunctionArgumentTypeStruct();
                arg.Name = ReadString();
                arg.Type = ReadInt();

                func.Arguments[i] = ReadFunctionArgumentType();
            }
            func.ReturnType = ReadInt();

            return func;
        }

        public FunctionArgumentTypeStruct ReadFunctionArgumentType()
        {
            var arg = new FunctionArgumentTypeStruct();
            arg.Name = ReadString();
            arg.Type = ReadInt();
            return arg;
        }

        public SymbolStruct ReadSymbol()
        {
            var symbol = new SymbolStruct();

            symbol.SymbolId = ReadInt();
            symbol.TypeId = ReadInt();
            symbol.FunctionId = ReadInt();
            if (symbol.FunctionId.Value == 0) symbol.FunctionId = null;
            symbol.Name = ReadString();

            return symbol;
        }

        public LocationStruct ReadLocation()
        {
            var location = new LocationStruct();

            location.Instructions.Start = ReadInt();
            location.Instructions.End = ReadInt();
            location.Source.Start = ReadInt();
            location.Source.End = ReadInt();

            return location;
        }

        #endregion
    }
}
