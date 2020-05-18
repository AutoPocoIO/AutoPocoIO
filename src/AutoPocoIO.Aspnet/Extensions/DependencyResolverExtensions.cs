using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using System;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.Extensions
{
    public static class DependencyResolverExtensions
    {
        public static T GetRequiredService<T>(this IDependencyResolver dependencyResolver)
        {
            Check.NotNull(dependencyResolver, nameof(dependencyResolver));
            var service = dependencyResolver.GetService(typeof(T));
            if (service == null)
                throw new ArgumentNullException(typeof(T).ToString(), ExceptionMessages.ServiceNotRegistered);
            return (T)service;
        }
    }
}
