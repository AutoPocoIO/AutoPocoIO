using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using System;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Add GetRequiredService to <see cref="IDependencyResolver"/>
    /// </summary>
    public static class DependencyResolverExtensions
    {
        /// <summary>
        /// Get a service from dependency resolver
        /// </summary>
        /// <typeparam name="T">Type of services</typeparam>
        /// <param name="dependencyResolver">Config resolver</param>
        /// <returns></returns>
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
