using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceActivator : IServiceActivator
    {
        private IEnumerable<ConstructorInfo> _constructors;
        private Type ImplementationType;

        public ServiceActivator(Type implementer)
        {
            ImplementationType = implementer;
        }


        public object Activate(IContainer container)
        {
            if (_constructors == null)
            {
                _constructors = ImplementationType.GetTypeInfo()
                                        .DeclaredConstructors
                                        .Where(c => c.IsPublic);
            }

            MappedConstructor constructor = PickConstrutor(container);
            return constructor.Activate();
        }

        private MappedConstructor PickConstrutor(IContainer container)
        {
            List<MappedConstructor> mappedConstructors = new List<MappedConstructor>();
            foreach (var constructor in _constructors)
            {
                MappedConstructor mappedConstructor = MappedConstructor.Map(constructor, container);
                if (mappedConstructor.IsValid)
                    mappedConstructors.Add(mappedConstructor);
            }

            if (!mappedConstructors.Any())
                throw new ArgumentException($"No valid constructors for {ImplementationType.FullName}.");
            else if (mappedConstructors.Count == 1)
                return mappedConstructors.First();
            else
            {
                var maxParameterConstrutors = mappedConstructors.GroupBy(
                    c => c.Constructor.GetParameters().Length,
                    c => c,
                    (key, grp) => new { ParamCount = key, Construtors = grp.ToList() })
                    .OrderByDescending(c => c.ParamCount)
                    .First();

                if (maxParameterConstrutors.Construtors.Count == 1)
                    return maxParameterConstrutors.Construtors.First();
                else
                    throw new ArgumentException($"Multiple constructors with the same number of parameters for type {mappedConstructors.First().Constructor.DeclaringType}");

            }
        }
    }
}
