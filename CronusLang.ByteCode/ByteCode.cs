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
            Write(header.CodeStartIndex);
            Write(header.CodeEndIndex);
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

        // TODO Redo
        public int Write(FunctionTypeLayout functionType)
        {
            int cursor = Cursor;

            Write(OpCode.DefFunc);
            Write(functionType.TypeId);
            Write(functionType.Arguments.Count());
            foreach (var arg in functionType.Arguments)
            {
                Write(arg.Name);
                Write(arg.Type);
            }
            Write(functionType.ReturnType);

            return cursor;
        }

        // TODO Redo
        public int Write(TypesIndex tableOfContents)
        {
            int cursor = Cursor;

            Write(tableOfContents.Entries.Length);
            foreach (var entry in tableOfContents.Entries)
            {
                Write(entry.TypeId);
                Write(entry.Position);
            }

            return cursor;
        }

        #region Read

        public HeaderStruct ReadHeader()
        {
            var header = new HeaderStruct();
            header.HeaderIndex = ReadInt();
            header.TypesIndex = ReadInt();
            header.FunctionsIndex = ReadInt();
            header.CodeStartIndex = ReadInt();
            header.CodeEndIndex = ReadInt();
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

        public FunctionTypeLayout ReadFunctionTypeLayout()
        {
            var func = new FunctionTypeLayout();

            // Must return DefFunc
            _ = ReadOpCode();
            func.TypeId = ReadInt();
            func.Arguments = new FunctionArgumentType[ReadInt()];
            for (int i = 0; i < func.Arguments.Length; i++)
            {
                var arg = new FunctionArgumentType();
                arg.Name = ReadString();
                arg.Type = ReadInt();

                func.Arguments[i] = arg;
            }
            func.ReturnType = ReadInt();

            return func;
        }

        public TypesIndex ReadTypesIndex()
        {
            var index = new TypesIndex();

            index.Entries = new TypesIndexEntry[ReadInt()];
            for (int i = 0; i < index.Entries.Length; i++)
            {
                var entry = new TypesIndexEntry();
                entry.TypeId = ReadInt();
                entry.Position = ReadInt();
                index.Entries[i] = entry;
            }

            return index;
        }

        #endregion
    }

    public struct HeaderStruct
    {
        public int HeaderIndex;

        public int TypesIndex;

        public int FunctionsIndex;

        public int CodeStartIndex;

        public int CodeEndIndex;

        public int TotalLength;
    }

    public struct FunctionTypeLayout
    {
        public int TypeId;

        public FunctionArgumentType[] Arguments;

        public int ReturnType;
    }

    public struct FunctionArgumentType
    {
        public string? Name;

        public int Type;
    }

    public struct TypesIndex
    {
        public TypesIndexEntry[] Entries;
    }

    public struct TypesIndexEntry
    {
        public int TypeId;

        public int Position;
    }

    public struct FunctionsIndex
    {
        public FunctionStruct[] Entries;
    }

    public struct FunctionStruct
    {
        public int FunctionId;

        public int Position;

        public int TypeId;

        public string[] ArgNames;

        public string Symbol;
    }
}
