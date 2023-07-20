﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Logic
{
    public class AndOp : BinaryOperator
    {
        public AndOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
