using Microsoft.Owin;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    /// <summary>
    /// Implmenet Owin Middlware that utilizes injected classes.
    /// </summary>
    public interface IOwinMiddlewareWithDI
    {
        /// <summary>
        /// Next middleware in the owin pipeline
        /// </summary>
        OwinMiddleware Next { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Invoke(IOwinContext context);
    }
}
