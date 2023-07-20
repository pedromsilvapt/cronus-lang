using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Arithmetic
{
    public class SubOp : BinaryOperator
    {
        public SubOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
