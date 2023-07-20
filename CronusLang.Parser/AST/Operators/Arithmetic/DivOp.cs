using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Arithmetic
{
    public class DivOp : BinaryOperator
    {
        public DivOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
