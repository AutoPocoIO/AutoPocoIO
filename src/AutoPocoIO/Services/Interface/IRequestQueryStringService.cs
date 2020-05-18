using System.Collections.Generic;

namespace AutoPocoIO.Services
{
    public interface IRequestQueryStringService
    {
        IDictionary<string, string> GetQueryStrings();
    }
}
