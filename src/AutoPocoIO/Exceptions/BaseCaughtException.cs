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
        public BaseCaughtException() => ResponseCode = HttpStatusCode.InternalServerError;

        public BaseCaughtException(HttpStatusCode httpStatus) => ResponseCode = httpStatus;

        public virtual HttpStatusCode ResponseCode { get; set; }
        public virtual string HttpErrorMessage => "InternalServerError";
    }
}
