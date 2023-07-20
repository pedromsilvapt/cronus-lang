using CronusLang.Compiler.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler
{
    public interface ISemanticComponent
    {
        bool Analyzed { get; set; }

        SST.Node SemanticNode { get; }

        List<SemanticDependency> Dependencies { get; }

        void Analyze(SemanticContext context);
    }
}
