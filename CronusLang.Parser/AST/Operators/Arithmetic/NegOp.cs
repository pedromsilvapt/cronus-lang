using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Arithmetic
{
    public class NegOp : UnaryOperator
    {
        public NegOp(Node right, LocationSpan location) : base(right, location)
        {
        }
    }
}
