using CronusLang.Compiler.Dependencies;
using CronusLang.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler
{
    public class SemanticProperty<T>
    {
        protected T? _value;

        public SST.Node Node { get; protected set; }

        public string PropertyName { get; protected set; }

        public bool IsResolved { get; protected set; }

        public SemanticDependency Dependency => new NodePropertyDependency(Node, PropertyName);

        public T Value
        {
            get
            {
                if (!IsResolved)
                {
                    throw new Exception("Trying to access not yet analyzed property.");
                }

                return _value!;
            }
        }

        public SemanticProperty(SST.Node node, string propertyName)
        {
            Node = node;
            PropertyName = propertyName;
            _value = default;
            IsResolved = false;
        }

        public bool TrackDependency(SemanticContext context, out T value)
        {
            if (!IsResolved)
            {
                context.RegisterDependency(Dependency);
                value = default!;
                return false;
            }
            else
            {
                value = _value!;
                return true;
            }
        }

        public bool TrackDependency(SemanticContext context)
        {
            return TrackDependency(context, out var _);
        }

        public void Resolve(SemanticContext context, T value)
        {
            if (IsResolved)
            {
                throw new Exception("Trying to access not yet analyzed property.");
            }

            _value = value;
            IsResolved = true;

            context.ResolveDependency(Dependency);
        }
    }
}
