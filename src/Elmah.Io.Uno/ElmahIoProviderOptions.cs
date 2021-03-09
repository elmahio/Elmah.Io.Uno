using Elmah.Io.Client.Models;
using System;

namespace Elmah.Io.Uno
{
    public class ElmahIoProviderOptions
    {
        /// <summary>
        /// Specify a filter function to be called before logging each message. If a filter returns true, the message isn't logged.
        /// </summary>
        public Func<CreateMessage, bool> OnFilter { get; set; }
        /// <summary>
        /// An application name to put on all error messages.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Specify an action to be called on all (not filtered) messages. Use this to decorate log messages with custom properties.
        /// </summary>
        public Action<CreateMessage> OnMessage { get; set; }
        /// <summary>
        /// Specify an action to be called on all (not filtered) messages if communication with the elmah.io API fails.
        /// </summary>
        public Action<CreateMessage, Exception> OnError { get; set; }
        /// <summary>
        /// Enable additional properties added manually and/or through ASP.NET Core, Elmah.Io.AspNetCore.ExtensionsLogging, and similar.
        /// </summary>
        public bool IncludeScopes { get; set; } = true;
    }
}
