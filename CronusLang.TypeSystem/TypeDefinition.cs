namespace CronusLang.TypeSystem
{
    
    public abstract class TypeDefinition
    {
        public int Id { get; set; } = -1;

        public SymbolIdentifier Symbol { get; set; }

        public TypeDefinition(SymbolIdentifier symbol)
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
}