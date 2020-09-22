using AutoPocoIO.Models;
using System;

namespace AutoPocoIO.Services
{
    public class AutoPocoServiceOptions
    {
        public Action<IServiceProvider, LogRequestAndResponseCommand, ILoggingService> OnLogging { get; set; }
        public Action<IServiceProvider, ILoggingService> OnLogged { get; set; }
    }
}
