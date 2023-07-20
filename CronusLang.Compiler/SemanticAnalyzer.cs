using AST = CronusLang.Parser.AST;
using SST = CronusLang.Compiler.SST;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CronusLang.Compiler.Dependencies;

namespace CronusLang.Compiler
{
    public class SemanticAnalyzer
    {
        public IReadOnlyList<SST.Node> SemanticNodes { get; set; }

        public TypesLibrary Library { get; set; }

        public List<DiagnosticMessage> Diagnostics { get; set; }

        protected Dictionary<SemanticDependency, HashSet<ISemanticComponent>> _dependencies;

        protected HashSet<SemanticDependency> _resolvedDependencies;

        protected Queue<ISemanticComponent> _nodesToAnalyze;

        public SemanticAnalyzer(TypesLibrary library, IReadOnlyList<SST.Node> semanticNodes)
        {
            Library = library;
            SemanticNodes = semanticNodes;
            Diagnostics = new List<DiagnosticMessage>();

            _dependencies = new Dictionary<SemanticDependency, HashSet<ISemanticComponent>>();
            _resolvedDependencies = new HashSet<SemanticDependency>();
            _nodesToAnalyze = new Queue<ISemanticComponent>();
        }

        /// <summary>
        /// Marks a dependency as resolved. After the analyze method finishes, all
        /// resolved dependencies will be checked, and any nodes that depend only 
        /// on them will be marked for re-analyzis.
        /// </summary>
        /// <param name="dependency"></param>
        public void ResolveDependency(SemanticDependency dependency)
        {
            _resolvedDependencies.Add(dependency);
        }

        protected void RegisterDependency(SemanticDependency dependency, ISemanticComponent dependant)
        {
            if (_dependencies.TryGetValue(dependency, out var set))
            {
                set.Add(dependant);
            }
            else
            {
                _dependencies.Add(dependency, new HashSet<ISemanticComponent> { dependant });
            }
        }

        protected bool Analyze(ISemanticComponent node)
        {
            // If this node was already analyzed, or if it still has dependencies, no need to analyze it again
            if (node.Analyzed)
            {
                return node.Analyzed;
            }

            // TODO Avoid creating a new object every time, reuse it instead
            node.Analyze(new SemanticContext(this, Library, node));

            if (!node.Dependencies.Any())
            {
                Console.WriteLine($"Analyzed node {node.GetType().Name}|{node.GetHashCode()}");

                node.Analyzed = true;

                // Mark this node as resolved, so anything that depends on it can be marked as resolved too
                ResolveDependency(new ComponentDependency(node));
            }
            else
            {
                Console.WriteLine($"Analyzed node {node.GetType().Name}|{node.GetHashCode()}" +
                    $"\n\tDependencies: { string.Join(", ", node.Dependencies) }" +
                    $"\n\tResolved: { string.Join(", ", _resolvedDependencies) }");

                // Register the new node dependencies created after calling the Analyze() method
                foreach (var dependency in node.Dependencies)
                {
                    RegisterDependency(dependency, node);
                }
            }

            // Handle dependencies resolved during the analysis of this node
            // To check what other nodes can be queued
            foreach (var resolvedDependency in _resolvedDependencies)
            {
                if (_dependencies.TryGetValue(resolvedDependency, out var dependantNodes))
                {
                    foreach (var dependant in dependantNodes)
                    {
                        bool removed = dependant.Dependencies.Remove(resolvedDependency);

                        // We do not have to worry about duplicated nodes in the queue, because:
                        //  - A node is only ever added to the queue if node.Dependencies is empty
                        //  - While a node is in the queue, dependencies can never be added to it
                        //    - New dependencies are only added to a node after it is Analyzed
                        //    - A node is always removed from the queue before being Analyzed
                        //if (removed && !dependant.Dependencies.Any())
                        //{
                        //    _nodesToAnalyze.Append(dependant);
                        //}

                        if (removed && !_nodesToAnalyze.Contains(dependant))
                        {
                            Console.WriteLine($"Queuing node {dependant.GetType().Name}|{dependant.GetHashCode()} because of {resolvedDependency}");

                            _nodesToAnalyze.Enqueue(dependant);
                        }
                    }

                    // Remove the dependency from our graph too
                    _dependencies.Remove(resolvedDependency);
                }
            }

            if (_resolvedDependencies.Count > 0)
            {
                _resolvedDependencies.Clear();
            }

            return node.Analyzed;
        }

        public bool Analyze()
        {
            foreach (var node in SemanticNodes)
            {
                // Initial call of Analyze on all nodes. This will create dependencies on those nodes.
                Analyze(node);
            }

            // Analyze all the queued nodes. These are nodes that were analyzed in the loop above, had dependencies,
            // but a subsequent node analysis resolved those dependencies, and as such, they were queued to be re-analyzed
            // This process can repeat again while analyzing the nodes in this loop, which means, the queue `_nodesToAnalyze`
            // can be bigger after calling Analyze than it was before calling it
            while (_nodesToAnalyze.Count > 0)
            {
                var node = _nodesToAnalyze.Dequeue();

                Analyze(node);
            }

            // If we have exhausted all the nodes queued to be analyzed (meaning, nodes with no unresolved dependencies)
            // but we will have nodes with dependencies listed, this means an error. It can be a node that depends on a
            // function that does not exist or a type that was not declared, for example
            if (_dependencies.Any())
            {
                foreach (var (dependency, dependantNodes) in _dependencies)
                {
                    foreach (var dependant in dependantNodes)
                    {
                        var message = dependency.GetUnresolvedDiagnostic(dependant.SemanticNode);

                        if (message != null)
                        {
                            Diagnostics.Add(message);
                        }
                    }
                }
            }

            return Diagnostics.Any(msg => msg.Level == DiagnosticLevel.Error);
        }

        //public TypeDefinition InferExpressionType(Node expression)
        //{

        //}

        ///// <summary>
        ///// Creates a Type Object representing the binding
        ///// </summary>
        ///// <param name="binding"></param>
        ///// <returns></returns>
        //public TypeDefinition InferBindingType(Binding binding)
        //{
        //    var
        //}

        //public TypeDefinition ResolveBindingType(Binding binding)
        //{
        //    var inferredType = InferBindingType(binding);
        //}

        //public SST.Expression AnalyzeExpression(AST.Node expression)
        //{
        //    if (expression is AST.Literals.IntLiteral expressionInt)
        //    {

        //    }
        //}

        //#region Analyze Expressions



        //#endregion

        //public SST.BindingType Analyze(AST.BindingType bindingTypeAST)
        //{
        //    var bindingType = new SST.BindingType(bindingTypeAST);



        //    return bindingType;
        //}

        //public SST.Binding Analyze(AST.Binding bindingAST)
        //{
        //    var binding = new SST.Binding(bindingAST);

        //    if (bindingAST.Signature != null)
        //    {
        //        binding.Signature = Analyze(bindingAST.Signature);
        //    }

        //    foreach (var childBindingAST in bindingAST.Bindings)
        //    {
        //        var childBinding = Analyze(childBindingAST);

        //        binding.Bindings.Add(childBinding);
        //    }

        //    binding.Expression = AnalyzeExpression(bindingAST.Expression);

        //    // Infer type. Will only work for bindings with zero arguments (properties)
        //    if (bindingAST.Signature == null)
        //    {
        //        throw new Exception("Type inference not yet implemented.");
        //    }

        //    return binding;
        //}

        //public SST.Script Analyze(AST.Script scriptAST)
        //{
        //    var script = new SST.Script(scriptAST);

        //    foreach (var childBindingAST in scriptAST.Bindings)
        //    {
        //        var childBinding = Analyze(childBindingAST);

        //        script.Bindings.Add(childBinding);
        //    }

        //    return script;
        //}
    }
}
