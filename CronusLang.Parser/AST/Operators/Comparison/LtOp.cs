using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Comparison
{
    public class LtOp : BinaryOperator
    {
        public LtOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
