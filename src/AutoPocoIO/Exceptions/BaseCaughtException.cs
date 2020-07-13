using System;
using System.Net;

namespace AutoPocoIO.Exceptions
{
    /// <summary>
    /// Base exception to show in logger
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2237:Mark ISerializable types with serializable", Justification = "<Pending>")]
    public abstract class BaseCaughtException : Exception//, ISerializable
    {
        /// <summary>
        /// Initialize exception with 500 status code
        /// </summary>
        public BaseCaughtException() => ResponseCode = HttpStatusCode.InternalServerError;
        /// <summary>
        /// Initialize exception with custom status code 
        /// </summary>
        /// <param name="httpStatus">Exception resonse code</param>
        public BaseCaughtException(HttpStatusCode httpStatus) => ResponseCode = httpStatus;
        /// <summary>
        /// Http Status code
        /// </summary>
        public virtual HttpStatusCode ResponseCode { get; set; }
        /// <summary>
        /// Short error message to explain status. Default is <c>"InternalServerError"</c>
        /// </summary>
        public virtual string HttpErrorMessage => "InternalServerError";
    }
}
