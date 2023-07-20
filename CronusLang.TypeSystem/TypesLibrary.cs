namespace CronusLang.TypeSystem
{
    public class TypesLibrary
    {
        protected int _functionIdCounter = 0;

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

            var id = _functionIdCounter++;
            try
            {
                TypesById[id] = type;

                type.Id = id;

                return type;
            }
            catch
            {
                // Revert the used Id
                _functionIdCounter = id;
                throw;
            }
        }
    }

    public abstract class TypeDefinition
    {
        public int Id { get; set; } = -1;

        public Symbol Symbol { get; set; }

        public TypeDefinition(Symbol symbol)
        {
            Symbol = symbol;
        }

        public abstract int GetSize();

        public abstract bool IsAssignableFrom(TypeDefinition other);

        public bool IsAssignableTo(TypeDefinition other)
        {
            return other.IsAssignableFrom(this);
        }
    }

    //public class NativeType<T> : TypeDefinition
    //{
    //    public NativeType(int id, Symbol symbol, System.Type type) : base(id, symbol)
    //    {
    //        BackingType = type;
    //    }

    //    public void ToBytes(T value, ReadOnlySpan<byte> bytes)
    //    {
    //        BitConverter.TryWriteBytes(bytes, value);
    //    }

    //    public void FromBytes(object value, ReadOnlySpan<byte> bytes);

    //    public override int GetSize()
    //    {
    //        return sizeof(T);
    //    }
    //}
}