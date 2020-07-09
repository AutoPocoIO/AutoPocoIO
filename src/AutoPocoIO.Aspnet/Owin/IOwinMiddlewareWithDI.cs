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
        /// Next middleware in the owin pipeline.
        /// </summary>
        OwinMiddleware NextComponent { get; set; }
        /// <summary>
        /// Action to be called in Owin pipeline.
        /// </summary>
        /// <param name="context">Current request's Owin envirnoment.</param>
        /// <returns></returns>
        Task Invoke(IOwinContext context);
    }
}
