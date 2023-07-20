﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST.Operators.Arithmetic
{
    public class AddOp : BinaryOperator
    {
        public AddOp(Node left, Node right) : base(left, right)
        {
        }
    }
}
