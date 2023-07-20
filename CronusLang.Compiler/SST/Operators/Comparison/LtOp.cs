﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler.SST.Operators.Comparison
{
    public class LtOp : BinaryOperator
    {
        public LtOp(SymbolsScope scope, AST.Operators.Comparison.LtOp syntaxNode, SemanticTransformer transformer) : base(scope, syntaxNode, transformer)
        {
        }

        public override void Analyze(SemanticContext context)
        {
            if (!Type.IsResolved)
            {
                Type.Resolve(context, context.BoolType);
            }
        }
    }
}
