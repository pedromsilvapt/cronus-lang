using CronusLang.Compiler.SST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Dependencies
{
    public class ComponentDependency : SemanticDependency
    {
        public ISemanticComponent Component { get; protected set; }

        public ComponentDependency(ISemanticComponent node) : base()
        {
            Component = node;
        }

        public override bool Equals(object? other)
        {
            ComponentDependency? dependency = other as ComponentDependency;

            if (dependency == null)
            {
                return false;
            }

            return dependency.Component == Component;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + GetType().GetHashCode();
            hash = (hash * 7) + Component.GetHashCode();
            return hash;
        }

        public override DiagnosticMessage? GetUnresolvedDiagnostic(Node dependant)
        {
            return null;
        }

        public override string ToString()
        {
            return $"NodeDependency({Component.GetType().Name}|{Component.GetHashCode()})";
        }
    }
}
