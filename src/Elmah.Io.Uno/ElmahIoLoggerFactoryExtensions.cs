using Elmah.Io.Uno;
using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class ElmahIoLoggerFactoryExtensions
    {
        /// <summary>
        /// TODO
        /// </summary>
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, options));
            return factory;
        }
    }
}
