using Elmah.Io.Uno;
using System;

namespace Microsoft.Extensions.Logging
{
    public static class ElmahIoLoggerFactoryExtensions
    {
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, options));
            return factory;
        }
    }
}
