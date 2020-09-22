using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPocoIO.Services
{
    public interface IReplaceServices<TClass>
    {
        IServiceCollection ReplaceInternalServices(IServiceProvider rootProvider, IServiceCollection services);
    }
}
