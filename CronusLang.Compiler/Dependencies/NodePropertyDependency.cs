using CronusLang.Compiler.SST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Dependencies
{
    public class NodePropertyDependency : SemanticDependency
    {
        public SST.Node Node { get; protected set; }

        public string PropertyName { get; protected set; }

        public NodePropertyDependency(SST.Node node, string propertyName) : base()
        {
            Node = node;
            PropertyName = propertyName;
        }

        public override bool Equals(object? other)
        {
            NodePropertyDependency? dependency = other as NodePropertyDependency;

            if (dependency == null)
            {
                return false;
            }

            return dependency.Node == Node && dependency.PropertyName == PropertyName;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + GetType().GetHashCode();
            hash = (hash * 7) + Node.GetHashCode();
            hash = (hash * 7) + PropertyName.GetHashCode();
            return hash;
        }

        public override DiagnosticMessage? GetUnresolvedDiagnostic(Node dependant)
        {
            return null;
        }

        public override string ToString()
        {
            return $"NodePropertyDependency({Node.GetType().Name}|{Node.GetHashCode()}, {PropertyName})";
        }
    }
}
