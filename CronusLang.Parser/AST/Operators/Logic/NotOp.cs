using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Logic
{
    public class NotOp : UnaryOperator
    {
        public NotOp(Node right, LocationSpan location) : base(right, location)
        {
        }
    }
}
