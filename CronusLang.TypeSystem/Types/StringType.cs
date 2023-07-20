using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.TypeSystem.Types
{
    public class StringType : TypeDefinition
    {
        public StringType(Symbol symbol) : base(symbol)
        {
        }

        public override int GetSize()
        {
            // A string is represented by two integers: the first one works as the address of the string, and
            // the second one represents the length (in terms of bytes) of the string
            return sizeof(int) * 2;
        }

        public override bool IsAssignableFrom(TypeDefinition other)
        {
            return other is StringType;
        }
    }
}
