using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Parser.AST
{
    public class BindingType : Node
    {
        public IList<BindingParameter> Parameters { get; protected set; }

        public TypeNode ReturnType { get; protected set; }

        public BindingType(IList<BindingParameter> parameters, TypeNode returnType, LocationSpan location) : base(location)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        public override int CountChildren()
        {
            return Parameters.Count + 1;
        }

        public override void GetChildren(Span<Node> childrenReceiver)
        {
            for (int i = 0; i < Parameters.Count; i++)
            {
                childrenReceiver[i] = Parameters[i];
            }
            
            childrenReceiver[Parameters.Count] = ReturnType;
        }

        public override Node SetChildren(ReadOnlySpan<Node> newChildren)
        {
            var parameters = new BindingParameter[newChildren.Length - 1];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = (BindingParameter)newChildren[i];
            }

            return new BindingType(parameters, (TypeNode)newChildren[newChildren.Length - 1], Location);
        }
    }
}
