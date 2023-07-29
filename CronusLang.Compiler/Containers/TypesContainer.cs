using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler.Containers
{
    public class TypesContainer
    {
        protected int _typeIdCounter = 1;

        public Dictionary<int, TypeDefinition> TypesById { get; set; } = new Dictionary<int, TypeDefinition>();
        // Types:
        // Int, UInt, Decimal, Bool, Null

        public T Register<T>(T type) where T : TypeDefinition
        {
            // If the type is already registered, no need to register it again
            if (type.Id >= 0)
            {
                return type;
            }

            var id = _typeIdCounter++;
            try
            {
                TypesById[id] = type;

                type.Id = id;

                return type;
            }
            catch
            {
                // Revert the used Id
                _typeIdCounter = id;
                throw;
            }
        }
    }

}
