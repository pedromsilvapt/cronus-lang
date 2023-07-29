using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.TypeSystem
{
    /// <summary>
    /// Immutable Symbol referencing some Type/Binding in the code
    /// </summary>
    public struct SymbolIdentifier
    {
        private string[] _segments;

        public IReadOnlyList<string> Segments => _segments;

        public IReadOnlyList<string> NamespaceSegments => _segments;

        public string FullPath => string.Join(".", Segments);

        public string Namespace => string.Join(".", NamespaceSegments);

        public string Name => Segments.Last();

        public SymbolIdentifier(string name)
        {
            _segments = name.Split(".").ToArray();
        }

        public SymbolIdentifier(params string[] segments) : this((IEnumerable<string>)segments)
        { }

        public SymbolIdentifier(IEnumerable<string> segments)
        {
            _segments = segments.SelectMany(seg => seg.Split(".")).ToArray();
        }

        public SymbolIdentifier(params SymbolIdentifier[] symbols) : this((IEnumerable<SymbolIdentifier>)symbols)
        { }

        public SymbolIdentifier(IEnumerable<SymbolIdentifier> symbols)
        {
            _segments = symbols.SelectMany(sym => sym.Segments).ToArray();
        }

        public static SymbolIdentifier operator +(SymbolIdentifier prefix, string name) => new SymbolIdentifier(prefix, name);

        public static implicit operator SymbolIdentifier(string name) => new SymbolIdentifier(name);

        public static implicit operator SymbolIdentifier(string[] segments) => new SymbolIdentifier(segments);
    }
}
