using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.TypeSystem.Types
{
    public class DecimalType : TypeDefinition
    {
        public DecimalType(SymbolIdentifier symbol) : base(symbol)
        {
        }

        public override int GetSize()
        {
            return sizeof(decimal);
        }

        public override bool IsAssignableFrom(TypeDefinition other)
        {
            return other is DecimalType || other is IntType;
        }
    }
}
