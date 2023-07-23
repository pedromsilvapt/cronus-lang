using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.TypeSystem.Types
{
    public class BoolType : TypeDefinition
    {
        public BoolType(Symbol symbol) : base(symbol)
        {
        }

        public override int GetSize()
        {
            return sizeof(bool);
        }

        public override bool IsAssignableFrom(TypeDefinition other)
        {
            return other is BoolType;
        }
    }
}
