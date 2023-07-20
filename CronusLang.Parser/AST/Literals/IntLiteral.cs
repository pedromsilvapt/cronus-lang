using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Literals
{
    public class IntLiteral : Literal<int>
    {
        public IntLiteral(int value, LocationSpan locationSpan) : base(value, locationSpan)
        {
        }
    }
}
