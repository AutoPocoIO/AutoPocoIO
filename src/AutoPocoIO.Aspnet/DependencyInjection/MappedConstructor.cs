using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoPoco.DependencyInjection
{
    internal class MappedConstructor
    {
        private List<Func<object>> _serviceFuncs;

        private MappedConstructor(ConstructorInfo constructor)
        {
            Constructor = constructor;
            IsValid = true;
        }

        public ConstructorInfo Constructor { get; }
        public bool IsValid { get; private set; }

        public static MappedConstructor Map(ConstructorInfo constructor, IContainer container)
        {
            MappedConstructor mappedConstructor = new MappedConstructor(constructor);
            var parameters = constructor.GetParameters();
            mappedConstructor._serviceFuncs = new List<Func<object>>();

            foreach (var parameter in parameters)
            {
                if (container.TryGetRegistration(parameter.ParameterType, out _))
                {
                    if (typeof(IEnumerable).IsAssignableFrom(parameter.ParameterType))
                        mappedConstructor._serviceFuncs.Add(() => container.GetServices(parameter.ParameterType.GenericTypeArguments[0]));
                    else
                        mappedConstructor._serviceFuncs.Add(() => container.GetService(parameter.ParameterType));
                }
                else
                {
                    mappedConstructor.IsValid = false;
                    break;
                }
            }

            return mappedConstructor;
        }

        public object Activate()
        {
            var args = new List<object>();
            foreach (var service in _serviceFuncs)
                args.Add(service.Invoke());

            var paramsInfo = Constructor.GetParameters();

            var parametersExpression = Expression.Parameter(typeof(object[]), "args");
            var argumentsExpression = new Expression[paramsInfo.Length];

            for (int paramIndex = 0; paramIndex < paramsInfo.Length; paramIndex++)
            {
                var indexExpression = Expression.Constant(paramIndex);
                var parameterType = paramsInfo[paramIndex].ParameterType;

                var parameterIndexExpression = Expression.ArrayIndex(parametersExpression, indexExpression);
                var convertExpression = Expression.Convert(parameterIndexExpression, parameterType);

                argumentsExpression[paramIndex] = convertExpression;
            }

            var newExpression = Expression.New(Constructor, argumentsExpression);
            var lambdaExpression = Expression.Lambda<Func<object[], object>>(newExpression, parametersExpression);

            var compiledExpression = lambdaExpression.Compile();
            return compiledExpression(args.ToArray());
        }
    }
}
