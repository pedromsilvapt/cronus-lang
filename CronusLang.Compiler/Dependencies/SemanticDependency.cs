using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Dependencies
{
    public abstract class SemanticDependency : IEquatable<object>, IEquatable<SemanticDependency>
    {
        public SemanticDependency()
        { }

        /// <summary>
        /// Generates a diagnostic message to show to the user whenever this dependency could 
        /// be resolved after the end of the analysis process. Expects a node to be passed
        /// indicating where this dependency existed, to allow reporting to the user.
        /// 
        /// Can return null, indicating that no message should be displayed as a result of this missing dependency.
        /// This can be useful in cases of Node Dependencies, for example, to avoid dependencies being throw
        /// all the way up the expression syntax tree because a leaf dependency was unresolved.
        /// </summary>
        /// <param name="dependant"></param>
        /// <returns></returns>
        public abstract DiagnosticMessage? GetUnresolvedDiagnostic(SST.Node dependant);

        public bool Equals(SemanticDependency? other)
        {
            return Equals((object?)other);
        }
    }
}
