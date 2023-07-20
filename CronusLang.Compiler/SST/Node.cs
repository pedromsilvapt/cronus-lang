using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;
using Sawmill;
using CronusLang.Compiler.Dependencies;
using Newtonsoft.Json;

namespace CronusLang.Compiler.SST
{
    public abstract class Node : IRewritable<SST.Node>, ISemanticComponent
    {
        public AST.Node SyntaxNode { get; set; }

        public SST.Node SemanticNode => this;

        public List<SemanticDependency> Dependencies { get; set; }

        public SymbolsScope Scope { get; set; }

        public bool Analyzed { get; set; } = false;

        public Node(SymbolsScope scope, AST.Node syntaxNode)
        {
            Scope = scope;
            SyntaxNode = syntaxNode;
            Dependencies = new List<SemanticDependency>();
        }

        public T GetSyntaxNode<T>() where T : AST.Node
        {
            return (T)SyntaxNode;
        }

        public abstract int CountChildren();

        public abstract void GetChildren(Span<Node> childrenReceiver);

        public abstract Node SetChildren(ReadOnlySpan<Node> newChildren);

        // To Be enabled & implmented in the future
        //public bool? IsConstant { get; protected set; }
        //public TypeDefinition? Type { get; protected set; }

        //public abstract IEnumerable<SemanticDependency> CalculateDependencies();

        public abstract void Analyze(SemanticContext context);

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
        }
    }
}
