using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Display;
using Windows.System.Profile;

namespace Elmah.Io.Uno
{
    /// <summary>
    /// TODO
    /// </summary>
    public class ElmahIoLogger : ILogger
    {
        private const string OriginalFormatPropertyKey = "{OriginalFormat}";
        private readonly IElmahioAPI _elmahioApi;
        private readonly Guid _logId;

        /// <summary>
        /// TODO
        /// </summary>
        public ElmahIoLogger(string apiKey, Guid logId, ElmahIoProviderOptions options)
        {
            _logId = logId;
            _elmahioApi = new ElmahioAPI(new ApiKeyCredentials(apiKey), HttpClientHandlerFactory.GetHttpClientHandler(new ElmahIoOptions()));
            _elmahioApi.Messages.OnMessage += (sender, args) => options.OnMessage?.Invoke(args.Message);
            _elmahioApi.Messages.OnMessageFail += (sender, args) => options.OnError?.Invoke(args.Message, args.Error);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Warning;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Title = message,
                Severity = LogLevelToSeverity(logLevel).ToString(),
                Data = new List<Item>(),
                ServerVariables = new List<Item>(),
            };

#pragma warning disable Uno0001 // Uno type or member is not implemented
            var os = (AnalyticsInfo.VersionInfo.DeviceFamily ?? "unknown").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            var osVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion ?? "unknown";
#pragma warning restore Uno0001 // Uno type or member is not implemented

            createMessage.ServerVariables.Add(new Item("User-Agent", $"X-ELMAHIO-MOBILE; OS={os}; OSVERSION={osVersion}; ENGINE=Uno"));

            try
            {
                var displayInfo = DisplayInformation.GetForCurrentView();
                if (displayInfo != null)
                {
                    var width = displayInfo.ScreenWidthInRawPixels;

                    var height = displayInfo.ScreenHeightInRawPixels;
                    var orientation = displayInfo.CurrentOrientation;
                    if (width > 0) createMessage.Data.Add(new Item("Screen-Width", width.ToString()));
                    if (height > 0) createMessage.Data.Add(new Item("Screen-Height", height.ToString()));
                    switch (orientation)
                    {
                        case DisplayOrientations.Landscape:
                        case DisplayOrientations.LandscapeFlipped:
                            createMessage.Data.Add(new Item("Screen-Orientation", "landscape"));
                            break;
                        case DisplayOrientations.Portrait:
                        case DisplayOrientations.PortraitFlipped:
                            createMessage.Data.Add(new Item("Screen-Orientation", "portrait"));
                            break;
                    }
                }
            }
            catch
            {
                // Could be called to early. Continue logging without screen data.
            }

            createMessage.Data.Add(new Item("X-ELMAHIO-DevicePlatform", os));
            createMessage.Data.Add(new Item("X-ELMAHIO-SEARCH-isMobile", true.ToString()));

            if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                foreach (var stateProperty in stateProperties.Where(prop => prop.Key != OriginalFormatPropertyKey))
                {
                    createMessage.Data.Add(stateProperty.ToItem());
                }
            }

            // Fill in as many blanks as we can by looking at environment variables, etc.
            if (string.IsNullOrWhiteSpace(createMessage.Source)) createMessage.Source = Source(exception);
            if (string.IsNullOrWhiteSpace(createMessage.Type)) createMessage.Type = Type(exception);

            if (exception != null)
            {
                createMessage.Detail = exception.ToString();
                foreach (var item in exception.ToDataList())
                {
                    createMessage.Data.Add(item);
                }
            }

            _elmahioApi.Messages.CreateAndNotify(_logId, createMessage);
        }

        private Severity LogLevelToSeverity(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return Severity.Fatal;
                case LogLevel.Debug:
                    return Severity.Debug;
                case LogLevel.Error:
                    return Severity.Error;
                case LogLevel.Information:
                    return Severity.Information;
                case LogLevel.Trace:
                    return Severity.Verbose;
                case LogLevel.Warning:
                    return Severity.Warning;
                default:
                    return Severity.Information;
            }
        }

        private string Source(Exception exception)
        {
            return exception?.GetBaseException().Source;
        }

        private string Type(Exception exception)
        {
            return exception?.GetBaseException().GetType().FullName;
        }
    }
}
