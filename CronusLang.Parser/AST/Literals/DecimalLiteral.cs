using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Literals
{
    public class DecimalLiteral : Literal<decimal>
    {
        public DecimalLiteral(decimal value, LocationSpan locationSpan) : base(value, locationSpan)
        {
        }
    }
}
