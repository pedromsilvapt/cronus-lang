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
    public struct Symbol
    {
        private string[] _segments;

        public IReadOnlyList<string> Segments => _segments;

        public IReadOnlyList<string> NamespaceSegments => _segments;

        public string FullPath => string.Join(".", Segments);

        public string Namespace => string.Join(".", NamespaceSegments);

        public string Name => Segments.Last();

        public Symbol(string name)
        {
            _segments = name.Split(".").ToArray();
        }

        public Symbol(params string[] segments) : this((IEnumerable<string>)segments)
        { }

        public Symbol(IEnumerable<string> segments)
        {
            _segments = segments.SelectMany(seg => seg.Split(".")).ToArray();
        }

        public Symbol(params Symbol[] symbols) : this((IEnumerable<Symbol>)symbols)
        { }

        public Symbol(IEnumerable<Symbol> symbols)
        {
            _segments = symbols.SelectMany(sym => sym.Segments).ToArray();
        }

        public static Symbol operator +(Symbol prefix, string name) => new Symbol(prefix, name);

        public static implicit operator Symbol(string name) => new Symbol(name);

        public static implicit operator Symbol(string[] segments) => new Symbol(segments);
    }
}
