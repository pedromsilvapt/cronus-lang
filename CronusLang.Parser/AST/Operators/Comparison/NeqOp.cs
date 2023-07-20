using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Comparison
{
    public class NeqOp : BinaryOperator
    {
        public NeqOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
