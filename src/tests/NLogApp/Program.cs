﻿using System;
using NLog;

namespace TestApp
{
    internal static class Program
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif
            log.Info($"Message from NLog Test App ({netfx})");
            App.Run();
        }
    }
}
