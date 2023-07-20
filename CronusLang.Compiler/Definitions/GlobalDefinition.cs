using CronusLang.ByteCode;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Definitions
{
    public class GlobalDefinition : InstructionsDefinition
    {
        public override bool IsGlobal => true;

        public override OpCode LoadOperation { get; set; } = OpCode.LoadGlobalN;

        public override OpCode StoreOperation { get; set; } = OpCode.StoreGlobalN;

        public GlobalDefinition() : base() { }
    }
}
