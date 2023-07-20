using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Comparison
{
    public class LteOp : BinaryOperator
    {
        public LteOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
