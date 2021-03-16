using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elmah.Io.Uno
{
    /// <summary>
    /// TODO
    /// </summary>
    public class ElmahIoLoggerProvider : ILoggerProvider
    {
        private readonly string _apiKey;
        private readonly Guid _logId;
        private readonly ElmahIoProviderOptions _options;

        /// <summary>
        /// TODO
        /// </summary>
        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            _apiKey = apiKey;
            _logId = logId;
            _options = options ?? new ElmahIoProviderOptions();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ILogger CreateLogger(string categoryName)
        {
            return new ElmahIoLogger(_apiKey, _logId, _options);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
        }
    }
}
