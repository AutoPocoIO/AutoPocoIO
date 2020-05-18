using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPocoIO.Services.Interface
{
    public interface IReplaceServices<TClass>
    {
        IServiceCollection ReplaceInternalServices(IServiceProvider rootProvider, IServiceCollection services);
    }
}
