using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;

namespace Elmah.Io.Uno
{
    public class ElmahIoLogger : ILogger
    {
        private const string OriginalFormatPropertyKey = "{OriginalFormat}";
        private readonly IElmahioAPI _elmahioApi;
        private readonly Guid _logId;

        public ElmahIoLogger(string apiKey, Guid logId, ElmahIoProviderOptions options)
        {
            _logId = logId;
            _elmahioApi = new ElmahioAPI(new ApiKeyCredentials(apiKey), HttpClientHandlerFactory.GetHttpClientHandler(new ElmahIoOptions()));
            _elmahioApi.Messages.OnMessage += (sender, args) => options.OnMessage?.Invoke(args.Message);
            _elmahioApi.Messages.OnMessageFail += (sender, args) => options.OnError?.Invoke(args.Message, args.Error);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Warning;
        }

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

            var os = "android"; // TODO: replace with dynamic OS name
            var osVersion = "9.0"; // TODO: replace with dynamic OS version

            // This doesn't work in class libraries
#if __ANDROID__
            os = "android";
#elif __IOS__
            os = "ios";
#endif

            createMessage.ServerVariables.Add(new Item("User-Agent", $"X-ELMAHIO-MOBILE; OS={os}; OSVERSION={osVersion}; ENGINE=Uno"));

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
