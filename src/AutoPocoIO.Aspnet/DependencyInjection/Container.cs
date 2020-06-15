using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoPoco.DependencyInjection
{
    internal class Container : IContainer
    {
        private IServiceRegistry _serviceRegistry;
        private readonly object _mutex = new object();
        private readonly ConcurrentDictionary<Guid, object> _sharedInstances = new ConcurrentDictionary<Guid, object>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public Container(IEnumerable<ServiceDescriptor> descriptors)
        {
            _serviceRegistry = new ServiceRegistry();
            foreach (var descriptor in descriptors)
            {
                var registration = new RegisteredService(descriptor);
                _serviceRegistry.AddRegistration(registration);
            }

            ServiceDescriptor containerDescriptor = new ServiceDescriptor(typeof(IContainer), c => this, ServiceLifetime.Singleton);
            var containerRegistration = new RegisteredService(containerDescriptor);
            _serviceRegistry.AddRegistration(containerRegistration);

            RootContainer = this;
            ContainerId = Guid.NewGuid();
        }

        private Container(IServiceRegistry componentRegistry, IContainer parent)
        {
            _serviceRegistry = componentRegistry;
            RootContainer = parent.RootContainer;
            ContainerId = Guid.NewGuid();
        }

        public IContainer RootContainer { get; }
        public Guid ContainerId { get; }

        public IContainer BeginScope()
        {
            var scope = new Container(_serviceRegistry, this);
            return scope;
        }

        public void Dispose()
        {
            foreach (var obj in _disposables)
                obj.Dispose();
        }

        public object GetService(Type serviceType)
        {
            //Requested a list
            if (typeof(IEnumerable).IsAssignableFrom(serviceType))
                return GetServices(serviceType.GenericTypeArguments[0]);
         
            if (_serviceRegistry.TryGetRegistration(serviceType, out var registration))
                return GetOrCreateInstance(serviceType, registration);

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var implementations = _serviceRegistry.GetServiceInfo(serviceType).Implementations;

            var parameter = Expression.Parameter(typeof(int));
            var newArray = Expression.NewArrayBounds(serviceType, parameter);
            var serviceListFunc = Expression.Lambda<Func<int, IList>>(newArray, parameter).Compile();

            IList services = serviceListFunc(implementations.Count);

            for(int i = 0; i < implementations.Count; i++)
                services[i] = GetOrCreateInstance(serviceType, implementations[i]);

            return (IEnumerable<object>)services;
        }

        public bool TryGetRegistration(Type type, out IRegistratedService registration)
        {
            return _serviceRegistry.TryGetRegistration(type, out registration);
        }

        private object GetOrCreateInstance(Type serviceType, IRegistratedService registration)
        {
            if (registration.Lifetime == ServiceLifetime.Singleton && ContainerId != RootContainer.ContainerId)
                return RootContainer.GetService(serviceType);
            if (TryGetSharedInstance(registration.Id, out var sharedInstance))
                return sharedInstance;
            else
            {
                if (registration.IsShared)
                {
                    var instance = CreateInstance(registration);
                    _sharedInstances.TryAdd(registration.Id, instance);
                    return instance;
                }
                else
                    return CreateInstance(registration);

            }
        }

        private object CreateInstance(IRegistratedService registration)
        {
            var instance = registration.Activate(this);

            if (instance is IDisposable disposable)
                _disposables.Add(disposable);

            return instance;
        }

        private bool TryGetSharedInstance(Guid id, out object value) =>
           _sharedInstances.TryGetValue(id, out value);
    }
}
