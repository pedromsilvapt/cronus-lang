using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler
{
    public class SemanticTransformer
    {
        private Dictionary<Type, Type> nodeTypeMapping;

        public SemanticTransformer(Dictionary<Type, Type> nodeTypeMapping)
        {
            this.nodeTypeMapping = nodeTypeMapping;
        }

        /// <summary>
        /// Assumes all possible types from the root node and all it's children nodes have a corresponding mapped SST Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootNode"></param>
        /// <param name="nodeTypeMapping"></param>
        /// <returns></returns>
        public T? TryToSST<T>(SymbolsScope scope, AST.Node? rootNode) where T : SST.Node
        {
            if (rootNode == null)
            {
                return null;
            }

            // The SST type we want to convert to.
            Type targetType = nodeTypeMapping[rootNode.GetType()];

            T result = (T)Activator.CreateInstance(targetType, new object[] { scope, rootNode, this })!;

            return result;
        }

        /// <summary>
        /// Assumes all possible types from the root node and all it's children nodes have a corresponding mapped SST Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootNode"></param>
        /// <param name="nodeTypeMapping"></param>
        /// <returns></returns>
        public IList<T> ToSST<T>(SymbolsScope scope, IEnumerable<AST.Node> sourceNodes) where T : SST.Node
        {
            return sourceNodes.Select(elem => ToSST<T>(scope, elem)).ToList();
        }

        /// <summary>
        /// Assumes all possible types from the root node and all it's children nodes have a corresponding mapped SST Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceNode"></param>
        /// <param name="nodeTypeMapping"></param>
        /// <returns></returns>
        public T ToSST<T>(SymbolsScope scope, AST.Node sourceNode) where T : SST.Node
        {
            // The SST type we want to convert to.
            Type targetType = nodeTypeMapping[sourceNode.GetType()];

            // We also pass the transformer as the second argument to the constructor
            // So that it can recursively create the SST child nodes too
            T result = (T)Activator.CreateInstance(targetType, new object[] { scope, sourceNode, this })!;

            return result;
        }

        public static SemanticTransformer FromReflection()
        {
            var astNamespace = typeof(AST.Node).Namespace!;
            var sstNamespace = typeof(SST.Node).Namespace!;

            // Get a list of all AST Node types
            var astTypes = typeof(AST.Node).Assembly.GetTypes()
                // First filter by the namepsace
                .Where(t => t.Namespace != null && t.Namespace.StartsWith(astNamespace, StringComparison.Ordinal))
                // And then by non-abstract classes only
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AST.Node)))
                // Group the types in a dictionary, where each of their keys is the full namespace and type name,
                // excluding the common prefix of the AST namespace
                // So for example, the class CronusLang.Parser.AST.Operators.Arithmetic.AddOp would be stored in the
                // key `Operations.Arithmetic.AddOp`
                .ToDictionary(t => string.Format("{0}.{1}", t.Namespace!, t.Name).Substring(astNamespace.Length + 1));

            // Get a list of all SST types
            var sstTypes = typeof(SST.Node).Assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.StartsWith(sstNamespace, StringComparison.Ordinal))
                // And then by non-abstract classes only
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(SST.Node)))
                // Group the types in a dictionary, where each of their keys is the full namespace and type name,
                // excluding the common prefix of the SST namespace
                // So for example, the class CronusLang.Compiler.SST.Operators.Arithmetic.AddOp would be stored in the
                // key `Operations.Arithmetic.AddOp`
                .ToDictionary(t => string.Format("{0}.{1}", t.Namespace!, t.Name).Substring(sstNamespace.Length + 1));

            // We don't particularly care if there are more classes under SST than AST
            // However, we want to enforce that there is a one-to-one mapping of all the AST node types,
            // so that we can make an automatic conversion between each one of them abd their corresponding SST node
            var missingSSTTypes = astTypes.Keys.Where(typeName => !sstTypes.ContainsKey(typeName)).ToList();

            if (missingSSTTypes.Any())
            {
                throw new Exception(String.Format("Missing SST node classes for the following AST node classes: {0}",
                    string.Join(", ", missingSSTTypes)));
            }

            // Create a Dictionary from Type to Type
            Dictionary<Type, Type> abstractToSemanticNodeMapping = astTypes.Select(kv => new
            {
                // The source type is the AST Node Type
                SourceType = kv.Value,
                // The target type is the corresponding SST Node Type with the same "namespace" and name
                // We can safely assume all keys exist because we made an explicit check for missing keys before
                TargetType = sstTypes[kv.Key]
            }).ToDictionary(map => map.SourceType, map => map.TargetType);

            return new SemanticTransformer(abstractToSemanticNodeMapping);
        }
    }
}
