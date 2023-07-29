using CronusLang.Compiler.Containers;
using CronusLang.Compiler.Dependencies;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler
{
    /// <summary>
    /// Hierarchical structure for registering and accessing symbols. All symbols declared in one scope
    /// are always available in child scopes, unless a symbol with the same identifier is explicitly defined
    /// in one of those child scopes. In such case, any references to that symbol will be shadowed, in both 
    /// the child scope, and in all of it's the child scopes as well.
    /// 
    /// Only the root scope has the reference for the Analyzer set, so when accessing it, 
    /// always prefix it with `RootScope.`
    /// 
    /// The ParentScope property **can** be null if the current scope **is the Root Scope**. However,
    /// the RootScope property is **never** null, even when the current scope is already the Root Scope. 
    /// In that case, it simply points to itself.
    /// 
    /// Usually the scopes are created by the SST Node constructors, when transforming the AST Node tree into
    /// the SST Node tree. Likewise, the **Reservation** of symbols is performed during the constraction. This 
    /// is simply a process of registering the names of the symbols that are declared in this scope. Only during
    /// the node's analyzis process will the actual symbols be registered, together with information about them.
    /// 
    /// Once a symbol is registered, the dependency to that symbol is resolved in this scope and recursively in all
    /// it's child scopes, stopping only on child scopes that shadow it.
    /// </summary>
    public class SymbolsScope
    {
        /// <summary>
        /// Reference to the Semantic Analyzer class.
        /// </summary>
        public SemanticAnalyzer Analyzer { get; set; }

        public SymbolsContainer SymbolsContainer { get; set; }

        public SymbolsScope RootScope { get; protected set; }

        public SymbolsScope? ParentScope { get; protected set; }

        public List<SymbolsScope> ChildrenScopes { get; protected set; }

        public bool Global { get; protected set; }

        public bool RequireReservation { get; protected set; }

        public bool RequireCapture { get; protected set; }

        public string? Name { get; protected set; }

        public string? FullName => _fullName.Value;

        protected HashSet<string> _reservedSymbols;

        protected Dictionary<string, Symbol> _symbols;

        protected Lazy<string?> _fullName;

        protected SymbolsScope(SemanticAnalyzer analyzer, SymbolsContainer symbolsContainer, string? name = null, bool global = true, bool requireReservation = true, bool requireCapture = false)
        {
            Analyzer = analyzer;
            SymbolsContainer = symbolsContainer;
            RootScope = this;
            ParentScope = null;
            ChildrenScopes = new List<SymbolsScope>();
            Name = name;
            Global = global;
            RequireReservation = requireReservation;
            RequireCapture = requireCapture;

            _reservedSymbols = new HashSet<string>();
            _symbols = new Dictionary<string, Symbol>();
            _fullName = new Lazy<string?>(() => CalculateFullName(), isThreadSafe: false);
        }

        protected SymbolsScope(SymbolsScope parentScope, string? name = null, bool? global = null, bool requireReservation = true, bool requireCapture = false)
        {
            Analyzer = parentScope.Analyzer;
            SymbolsContainer = parentScope.SymbolsContainer;
            ParentScope = parentScope;
            RootScope = parentScope.RootScope;
            ChildrenScopes = new List<SymbolsScope>();
            Name = name;
            Global = global ?? parentScope.Global;
            RequireReservation = requireReservation;
            RequireCapture = requireCapture;

            _reservedSymbols = new HashSet<string>();
            _symbols = new Dictionary<string, Symbol>();
            _fullName = new Lazy<string?>(() => CalculateFullName(), isThreadSafe: false);

            parentScope.ChildrenScopes.Add(this);
        }

        protected string? CalculateFullName()
        {
            string? fullName = ParentScope?.FullName;

            if (Name != null && fullName != null)
            {
                return fullName + "." + Name;
            }
            else if (Name != null)
            {
                return Name;
            }
            else
            {
                return fullName;
            }
        }

        public void Reserve(string identifier)
        {
            Console.WriteLine($"Reserving {identifier} in {FullName}");
            _reservedSymbols.Add(identifier);
        }

        public bool IsSymbolReserved(string identifier)
        {
            return _reservedSymbols.Contains(identifier);
        }

        public void Register(string identifier, Symbol symbol)
        {
            // Captures do not need to be reserved
            if (RequireReservation && !symbol.IsCapture && !IsSymbolReserved(identifier))
            {
                throw new Exception(string.Format("Cannot register un-reserved symbol {0}", identifier));
            }

            SymbolsContainer.Register(identifier, symbol);

            if (!_symbols.ContainsKey(identifier))
            {
                _symbols.Add(identifier, symbol);

                // No need to announce captures, since thay have already been announced by the original symbol
                if (!symbol.IsCapture)
                {
                    ResolveChildDependencies(new SymbolDependency(this, identifier));
                }
            }
            else
            {
                _symbols[identifier] = symbol;
            }
        }

        public void RegisterCapture(string sourceIdentifier, SymbolsScope sourceScope, string identifier, TypeDefinition type)
        {
            Register(sourceIdentifier, SymbolsContainer.CreateCapture(sourceScope, identifier, type));
        }
        
        public void RegisterParameter(string identifier, int parameterIndex, TypeDefinition type)
        {
            Register(identifier, SymbolsContainer.CreateParameter(parameterIndex, type));
        }
        
        public void RegisterBinding(string identifier, int bindingIndex, bool global, TypeDefinition type)
        {
            Register(identifier, SymbolsContainer.CreateBinding(bindingIndex, global, type));
        }

        public void RegisterType(TypeDefinition type)
        {
            Register(type.Symbol.Name, SymbolsContainer.CreateType(type));
        }

        public Symbol? TryLookup(string identifier, LookupOptions lookupOptions = LookupOptions.Self)
        {
            SymbolsScope? startingScope = this;

            if (lookupOptions == LookupOptions.Root)
            {
                startingScope = this.RootScope;
            }
            else if (lookupOptions == LookupOptions.Parent || lookupOptions == LookupOptions.ParentOnly)
            {
                startingScope = ParentScope;
            }

            Symbol? result = null;

            Stack<SymbolsScope> capturingScopesList = new Stack<SymbolsScope>();

            while (startingScope != null)
            {
                if (startingScope._symbols.TryGetValue(identifier, out var symbol))
                {
                    if (!symbol.IsBinding || !symbol.BindingGlobal!.Value)
                    {
                        var sourceScope = startingScope;

                        // Create the capture symbols in the collected intermediary scopes (if there are any)
                        // and in the end we will want to return the last captured symbol, instead of the original
                        while (capturingScopesList.Count > 0)
                        {
                            symbol = SymbolsContainer.CreateCapture(
                                sourceScope,
                                identifier,
                                symbol.Type
                            );

                            sourceScope = capturingScopesList.Pop();

                            sourceScope.Register(identifier, symbol);
                        }
                    }

                    result = symbol;
                    break;
                }
                else if (lookupOptions == LookupOptions.SelfOnly || lookupOptions == LookupOptions.ParentOnly)
                {
                    break;
                }
                else if (startingScope._reservedSymbols.Contains(identifier))
                {
                    // Do not look up any further if the symbol is reserved, but has not yet been declared
                    // If it has been reserved, it is a guarantee that it will be declared eventually in this scope
                    // and in doing so, it will shadow whatever symbol we could potentially find by continuing the
                    // search up above, and in doing so it means whatever we could find now would be an invalid result
                    break;
                }
                else
                {
                    // If this scope requires capture, and we have not yet found the symbol,
                    // we can continue to lookup above, but we have to remember this scope
                    // so when we finally find the symbol, we can create the intermediary
                    // capture symbol entries in all the intermediary scopes that require capture
                    if (startingScope.RequireCapture)
                    {
                        capturingScopesList.Push(startingScope);
                    }

                    startingScope = startingScope.ParentScope;
                }
            }

            return result;
        }

        public Symbol Lookup(string identifier, LookupOptions lookupOptions = LookupOptions.Self)
        {
            var symbol = TryLookup(identifier, lookupOptions);

            if (symbol == null)
            {
                throw new Exception($"No symbol named {identifier} found.");
            }

            return symbol;
        }

        public TypeDefinition? TryLookupType(string identifier, LookupOptions lookupOptions = LookupOptions.Self)
        {
            var symbol = TryLookup(identifier, lookupOptions);

            if (symbol == null)
            {
                return null;
            }

            if (!symbol.IsType)
            {
                throw new Exception($"Symbol {identifier} is not a symbol.");
            }

            return symbol.Type!;
        }

        public TypeDefinition LookupType(string identifier, LookupOptions lookupOptions = LookupOptions.Self)
        {
            var type = TryLookupType(identifier, lookupOptions);

            if (type == null)
            {
                throw new Exception($"No type named {identifier} found.");
            }

            return type;
        }

        protected void ResolveChildDependencies(SymbolDependency symbolDependency)
        {
            if (RootScope.Analyzer != null)
            {
                // TODO How to handle inherited symbols?
                RootScope.Analyzer.ResolveDependency(symbolDependency);

                foreach (var child in ChildrenScopes)
                {
                    // Since symbols declared in children scopes can shadow symbols declared in ancestor scopes
                    // if they have the same identifier, we only want to resolve the dependency in child scopes that
                    // do not shadow this symbol
                    if (!child.IsSymbolReserved(symbolDependency.Identifier))
                    {
                        child.ResolveChildDependencies(new SymbolDependency(child, symbolDependency.Identifier));
                    }
                }
            }
        }

        public SymbolsScope CreateChild(string? name = null, bool? global = null, bool requireReservation = true, bool requireCapture = false)
        {
            return new SymbolsScope(this, name, global, requireReservation, requireCapture);
        }

        public static SymbolsScope CreateRoot(SemanticAnalyzer analyzer, SymbolsContainer symbolsContainer, string? name = null, bool global = true, bool requireReservation = false, bool requireCapture = false)
        {
            return new SymbolsScope(analyzer, symbolsContainer, name, global, requireReservation, requireCapture);
        }
    }

    public enum LookupOptions
    {
        /// <summary>
        /// Lookup the symbol under the current scope and up
        /// </summary>
        Self,

        /// <summary>
        /// Lookup the symbol under the current scope and up
        /// </summary>
        SelfOnly,

        /// <summary>
        /// Lookup the symbol under the parent scope (if any) and up
        /// </summary>
        Parent,

        /// <summary>
        /// Lookup the symbol under the parent scope (if any) and up
        /// </summary>
        ParentOnly,

        /// <summary>
        /// Lookup the symbol under the root scope only
        /// </summary>
        Root
    }

    // TODO
    public class Symbol
    {
        #region Parameter variant

        public static Symbol CreateParameter(int symbolId, int parameterIndex, TypeDefinition type)
        {
            return new Symbol
            {
                SymbolId = symbolId,
                IsParameter = true,
                ParameterIndex = parameterIndex,
                Type = type,
            };
        }

        public bool IsParameter { get; set; } = false;

        public int? ParameterIndex { get; set; } = null;

        #endregion

        #region Capture variant

        public static Symbol CreateCapture(int symbolId, SymbolsScope sourceScope, string identifier, TypeDefinition type)
        {
            return new Symbol
            {
                SymbolId = symbolId,
                IsCapture = true,
                CaptureSourceScope = sourceScope,
                CaptureIdentifier = identifier,
                Type = type,
            };
        }

        public bool IsCapture { get; set; } = false;

        public SymbolsScope? CaptureSourceScope { get; set; } = null;

        public string? CaptureIdentifier { get; set; } = null;

        #endregion

        #region Binding variant

        public static Symbol CreateBinding(int symbolId, int bindingIndex, bool global, TypeDefinition type)
        {
            return new Symbol
            {
                SymbolId = symbolId,
                IsBinding = true,
                BindingIndex = bindingIndex,
                BindingGlobal = global,
                Type = type,
            };
        }

        public bool IsBinding { get; set; } = false;

        public int? BindingIndex { get; set; } = null;

        public bool? BindingGlobal { get; set; } = null;

        #endregion

        #region Type variant

        public static Symbol CreateType(int symbolId, TypeDefinition type)
        {
            return new Symbol
            {
                SymbolId = symbolId,
                IsType = true,
                Type = type,
            };
        }

        public bool IsType { get; set; } = false;

        #endregion

        #region Global fields

        public int SymbolId { get; set; } = 0;

        public TypeDefinition Type { get; set; } = null!;

        #endregion
    }
}
