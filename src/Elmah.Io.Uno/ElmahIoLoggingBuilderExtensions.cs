﻿using System;
using Elmah.Io.Uno;

namespace Microsoft.Extensions.Logging
{
    public static class ElmahIoLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder, string apiKey, Guid logId)
        {
            builder.AddProvider(new ElmahIoLoggerProvider(apiKey, logId));
            return builder;
        }

        //public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder)
        //{
        //    builder.Services.AddSingleton<ILoggerProvider, ElmahIoLoggerProvider>(services =>
        //    {
        //        var options = services.GetService<IOptions<ElmahIoProviderOptions>>();
        //        return new ElmahIoLoggerProvider(options);
        //    });
        //    return builder;
        //}

    }
}
