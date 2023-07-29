using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.TypeSystem.Types
{
    public class FunctionTypeDefinition : TypeDefinition
    {
        public List<FunctionTypeArgumentDefinition> Arguments { get; set; }

        public TypeDefinition ReturnType { get; set; }

        public FunctionTypeDefinition(SymbolIdentifier symbol, List<FunctionTypeArgumentDefinition> arguments, TypeDefinition returnType) : base(symbol)
        {
            Arguments = arguments;
            ReturnType = returnType;
        }

        public override int GetSize()
        {
            // We will represent functions by their Ids, and as such, they are integers
            return sizeof(int) * 2;
        }

        public override bool IsAssignableFrom(TypeDefinition other)
        {
            if (other is FunctionTypeDefinition otherFunction)
            {
                if (Arguments.Count != otherFunction.Arguments.Count)
                {
                    return false;
                }

                // The arguments types are compared with "IsAssignableTo" because the other function's arguments must be equal or
                // more generic/broad that this one's. If this function expects an Animal, it cannot be replaced by a function
                // that accepts a Pet, because not all Animals are Pets. However, it could accept a Being, because all Animals are beings
                if (Arguments.Zip(otherFunction.Arguments).Any(arg => !arg.First.Type.IsAssignableTo(arg.Second.Type)))
                {
                    return false;
                }

                // However the return type works the other way around, with "IsAssignableFrom", because if this function returns an Animal,
                // then we can replace it with a function that returns Pets, because all Pets are Animals. However, we could not use
                // a function that returns Beings, because not all Beings are Animals.
                if (!ReturnType.IsAssignableFrom(otherFunction.ReturnType))
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FunctionTypeArgumentDefinition
    {
        public TypeDefinition Type { get; set; }

        public FunctionTypeArgumentDefinition(TypeDefinition type)
        {
            Type = type;
        }
    }
}
