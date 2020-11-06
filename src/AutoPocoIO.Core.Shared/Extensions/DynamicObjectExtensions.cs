using AutoPocoIO.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using DynamicExpression = System.Linq.Dynamic.Core.DynamicExpressionParser;
using System.Linq.AutoPoco;
using System.Collections;

namespace AutoPocoIO.Extensions
{

    internal static partial class DynamicObjectExtensions
    {
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static object JTokenToConventionalDotNetObject(this JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ((JObject)token).Properties()
                        .ToDictionary(prop => prop.Name, prop => JTokenToConventionalDotNetObject(prop.Value));
                case JTokenType.Array:
                    return token.Values().Select(JTokenToConventionalDotNetObject).ToList();
                default:
                    return token.ToObject<object>();
            }
        }

        public static void PopulateObjectFromJToken(this JToken value, object target)
        {
            using (var sr = value.CreateReader())
            {
                JsonSerializer.CreateDefault().Populate(sr, target); // Uses the system default JsonSerializerSettings
            }
        }

        public static T PopulateModel<T>(this object source) where T : class
        {
            if (source == null)
                return null;

            T entity;
            if (typeof(T).GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                entity = Activator.CreateInstance<T>();
            else
            {
                var annonProperties = typeof(T).GetProperties();
                var values = new List<object>();
                foreach (var propertyInfo in annonProperties)
                {
                    values.Add(propertyInfo.GetValue(source));
                }

                entity = (T)Activator.CreateInstance(typeof(T), values.ToArray());
            }

            var sourcetype = source.GetType();

            var properties = typeof(T).GetProperties().Where(c => c.CanWrite);

            foreach (var propertyInfo in properties)
            {
                var sourcePropertyInfo = sourcetype.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourcePropertyInfo != null)
                    propertyInfo.SetValue(entity, sourcePropertyInfo.GetValue(source), null);
            }

            return entity;
        }

        public static dynamic PopulateModel(object source, Type modelType)
        {
            if (source == null)
                return null;

            var entity = Activator.CreateInstance(modelType);
            var sourcetype = source.GetType();

            var properties = modelType.GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo != null)
                {
                    var sourcePropertyInfo = sourcetype.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (sourcePropertyInfo != null)
                        propertyInfo.SetValue(entity, sourcePropertyInfo.GetValue(source), null);
                }
            }

            return entity;
        }

        public static void PopulateModel(this object source, object model)
        {
            var properties = model.GetType().GetProperties().Where(c => c.CanWrite);
            var sourcetype = source.GetType();
            foreach (var propertyInfo in properties)
            {
                var sourcePropertyInfo = sourcetype.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourcePropertyInfo != null)
                    propertyInfo.SetValue(model, sourcePropertyInfo.GetValue(source), null);
            }
        }






        public static IQueryable Join(this IQueryable outer, IQueryable inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotEmpty(outerSelector, nameof(outerSelector));
            Check.NotEmpty(innerSelector, nameof(innerSelector));
            Check.NotEmpty(resultsSelector, nameof(resultsSelector));

            var outerParameter = Expression.Parameter(outer.ElementType, "outer");
            var innerParameter = Expression.Parameter(inner.ElementType, "inner");

            LambdaExpression outerSelectorLambda = DynamicExpression.ParseLambda(new[] { outerParameter }, null, outerSelector, values);
            LambdaExpression innerSelectorLambda = DynamicExpression.ParseLambda(new[] { innerParameter }, null, innerSelector, values);

            ParameterExpression[] parameters = new ParameterExpression[] {
            Expression.Parameter(outer.ElementType, "outer"), Expression.Parameter(inner.ElementType, "inner") };
            LambdaExpression resultsSelectorLambda = DynamicExpression.ParseLambda(parameters, null, resultsSelector, values);

            return outer.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Join",
                    new Type[] { outer.ElementType, inner.AsQueryable().ElementType, outerSelectorLambda.Body.Type, resultsSelectorLambda.Body.Type },
                    outer.Expression, inner.AsQueryable().Expression, Expression.Quote(outerSelectorLambda), Expression.Quote(innerSelectorLambda), Expression.Quote(resultsSelectorLambda)));
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            Check.NotNull(source, nameof(source));

            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
        //The generic overload.
        public static IQueryable<T> Join<T>(this IQueryable<T> outer, IQueryable<T> inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
        {
            return (IQueryable<T>)Join((IQueryable)outer, (IQueryable)inner, outerSelector, innerSelector, resultsSelector, values);
        }

        public static IQueryable GroupJoin(this IQueryable outer, IQueryable inner, string outerKeySelector, string innerKeySelector, string resultSelector, params object[] values)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotEmpty(outerKeySelector, nameof(outerKeySelector));
            Check.NotEmpty(innerKeySelector, nameof(innerKeySelector));
            Check.NotEmpty(resultSelector, nameof(resultSelector));

            Type innerElementType = inner.AsQueryable().ElementType;

            var outerParameter = Expression.Parameter(outer.ElementType, "outer");
            var innerParameter = Expression.Parameter(innerElementType, "inner");
            var groupParameter = Expression.Parameter(typeof(IEnumerable<>)
                .MakeGenericType(innerElementType), "group");



            var outerLambda = DynamicExpression.ParseLambda(new[] { outerParameter },
                null, outerKeySelector, values);

            var innerLambda = DynamicExpression.ParseLambda(new[] { innerParameter },
               outerLambda.Body.Type, innerKeySelector, values);

            var resultLambda = DynamicExpression.ParseLambda(new[] { outerParameter, groupParameter },
                null, resultSelector, values);

            return outer.Provider.CreateQuery(Expression.Call(typeof(Queryable), "GroupJoin",
                new[] { outer.ElementType, innerElementType, outerLambda.Body.Type, resultLambda.Body.Type },
                outer.Expression, Expression.Constant(inner),
                Expression.Quote(outerLambda), Expression.Quote(innerLambda),
                Expression.Quote(resultLambda)));

        }

#if EF22
             public static IEnumerable<T> LeftJoin<T>(this IQueryable<T> outer, IQueryable<T> inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
             {
                    return GroupJoins<T>(outer, inner, outerSelector, innerSelector, resultsSelector);
             }
#else


        public static (IEnumerable<object> results, Type resultType) LeftJoinWithResultType(this IEnumerable outer, IEnumerable inner, string outerKeySelector, string innerKeySelector, string resultSelector, params object[] values)
        {
            Type innerElementType = inner.AsQueryable().ElementType;

            var outerParameter = Expression.Parameter(outer.AsQueryable().ElementType, "outer");
            var groupParameter = Expression.Parameter(typeof(IEnumerable<>)
                .MakeGenericType(innerElementType), "group");

            LambdaExpression resultLambda = DynamicExpression.ParseLambda(new[] { outerParameter, groupParameter },
                null, resultSelector, values);

            return (outer.LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, values), resultLambda.Body.Type);
        }

        public static IQueryable<object> LeftJoin(this IEnumerable outer, IEnumerable inner, string outerKeySelector, string innerKeySelector, string resultSelector, params object[] values)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(inner, nameof(inner));
            Check.NotEmpty(outerKeySelector, nameof(outerKeySelector));
            Check.NotEmpty(innerKeySelector, nameof(innerKeySelector));

            Check.NotEmpty(resultSelector, nameof(resultSelector));

            Type innerElementType = inner.AsQueryable().ElementType;

            var outerParameter = Expression.Parameter(outer.AsQueryable().ElementType, "outer");
            var innerParameter = Expression.Parameter(innerElementType, "inner");
            var groupParameter = Expression.Parameter(typeof(IEnumerable<>)
                .MakeGenericType(innerElementType), "group");

            LambdaExpression outerLambda = DynamicExpression.ParseLambda(new[] { outerParameter },
               null, outerKeySelector, values);

            LambdaExpression innerLambda = DynamicExpression.ParseLambda(new[] { innerParameter },
               outerLambda.Body.Type, innerKeySelector, values);

            LambdaExpression resultLambda = DynamicExpression.ParseLambda(new[] { outerParameter, groupParameter },
                null, resultSelector, values);

            MethodInfo method = typeof(Enumerable).GetMethods().First(c => c.Name == "ToLookup")
                .MakeGenericMethod(new[] { innerElementType, outerLambda.Body.Type });

            MethodInfo containsMethod = typeof(DynamicObjectExtensions).GetMethod(nameof(DynamicContans), BindingFlags.Static | BindingFlags.NonPublic)
                 .MakeGenericMethod(new[] { outerLambda.Body.Type, innerElementType });

            var lookup = Expression.Lambda(Expression.Call(method,
              Expression.Constant(inner),
              innerLambda)).Compile().DynamicInvoke();

            List<object> retList = new List<object>();
            foreach(var outerElement in outer)
            {
                var key = outerLambda.Compile().DynamicInvoke(outerElement);
                var val = Expression.Lambda(Expression.Call(containsMethod,
                                            Expression.Constant(lookup),
                                            Expression.Convert(Expression.Constant(key), outerLambda.Body.Type)))
                                    .Compile()
                                    .DynamicInvoke();

                retList.Add(resultLambda.Compile().DynamicInvoke(outerElement, val));

            }

            return (IQueryable<object>)System.Linq.Dynamic.Core.DynamicQueryableExtensions.OfType(retList.AsQueryable(), resultLambda.Body.Type);
        }

        private static IEnumerable<TSource> DynamicContans<TKey, TSource>(ILookup<TKey, TSource> lookup, TKey key)
        {
            return lookup.Contains(key) ? lookup[key] : new List<TSource>();
        }

#endif

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">dynamic list</typeparam>
        /// <param name="outer">outer side of the join</param>
        /// <param name="inner">inner side of the join</param>
        /// <param name="outerSelector">key value to join on. Example 1: outer.id, Example 2: new(outer.id, outer.name) </param>
        /// <param name="innerSelector">key value to join on. Example 1: inner.id, Example 2: new(inner.id, inner.name)</param>
        /// <param name="resultsSelector">How to display results. Example : new(group as Address, outer.name as Homeowner)</param>
        /// <param name="values">Parameters</param>
        /// <returns></returns>
        public static IQueryable<T> GroupJoin<T>(this IQueryable<T> outer, IQueryable<T> inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
        {
            return (IQueryable<T>)GroupJoin((IQueryable)outer, (IQueryable)inner, outerSelector, innerSelector, resultsSelector, values);
        }
    }

    public class CountTotal
    {
        public int Count { get; set; }
    }
}