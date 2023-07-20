using CronusLang.ByteCode;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Definitions
{
    public class SymbolDefinition
    {
        public bool IsGlobal { get; set; }

        public int Index { get; set; }

        public TypeDefinition Type { get; set; }

        public List<Instruction> Instructions { get; set; }

        public OpCode LoadOperation { get; set; }

        public OpCode StoreOperation { get; set; }

        public SymbolDefinition(bool isGlobal, OpCode loadOp, OpCode storeOp, int index, TypeDefinition type)
        {
            IsGlobal = isGlobal;
            Index = index;
            Type = type;
            LoadOperation = loadOp;
            StoreOperation = storeOp;
            Instructions = new List<Instruction>();
        }
    }
}
