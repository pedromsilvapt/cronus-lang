using CronusLang.Compiler.Dependencies;
using CronusLang.TypeSystem;
using CronusLang.TypeSystem.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AST = CronusLang.Parser.AST;

namespace CronusLang.Compiler
{
    public class SemanticContext
    {
        public SemanticAnalyzer Analyzer { get; protected set;  }

        public TypesLibrary Types { get; protected set;  }

        public ISemanticComponent Component { get; set; }

        public SemanticContext(SemanticAnalyzer analyzer, TypesLibrary types, ISemanticComponent component)
        {
            Types = types;
            Component = component;
            Analyzer = analyzer;
        }

        public void RegisterDependency(SemanticDependency dependency)
        {
            if (!Component.Dependencies.Contains(dependency))
            {
                Component.Dependencies.Add(dependency);
            }
        }

        public void ResolveDependency(SemanticDependency dependency)
        {
            Analyzer.ResolveDependency(dependency);
        }

        public void Emit(DiagnosticMessage message)
        {
            Analyzer.Diagnostics.Add(message);
        }

        public void Emit(DiagnosticLevel level, string message)
        {
            Emit(new DiagnosticMessage(level, message, Component.SemanticNode?.GetSyntaxNode<AST.Node>()?.Location));
        }

        public BoolType BoolType => (BoolType)Component.SemanticNode.Scope.LookupType("Bool", LookupOptions.Root);

        public IntType IntType => (IntType)Component.SemanticNode.Scope.LookupType("Int", LookupOptions.Root);

        public DecimalType DecimalType => (DecimalType)Component.SemanticNode.Scope.LookupType("Decimal", LookupOptions.Root);

        public StringType StringType => (StringType)Component.SemanticNode.Scope.LookupType("String", LookupOptions.Root);
    }
}
