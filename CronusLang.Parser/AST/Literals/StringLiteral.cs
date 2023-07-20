using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Literals
{
    public class StringLiteral : Literal<string>
    {
        public StringLiteral(string value, LocationSpan locationSpan) : base(value, locationSpan)
        {
        }
    }
}
