using System;
using System.Collections.Generic;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;

namespace TestApp
{
    // Common.Logging does not provide a Console logger when targetting.NET Core.So here it is (adapted from Common.Logging netfx version):
    internal sealed class ConsoleOutLoggerFactoryAdapter : AbstractSimpleLoggerFactoryAdapter
    {
        public ConsoleOutLoggerFactoryAdapter(LogLevel level, bool showDateTime, bool showLogName, bool showLevel, string dateTimeFormat, bool useColor) :
            base(level, showDateTime, showLogName, showLevel, dateTimeFormat) => UseColor = useColor;

        private bool UseColor { get; }

        protected override ILog CreateLogger(string name, LogLevel level, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat) =>
            new ConsoleOutLogger(name, level, showLevel, showDateTime, showLogName, dateTimeFormat, UseColor);
    }

    internal sealed class ConsoleOutLogger : AbstractSimpleLogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> colors = new Dictionary<LogLevel, ConsoleColor>
        {
            { LogLevel.Fatal, ConsoleColor.Red },
            { LogLevel.Error, ConsoleColor.Yellow },
            { LogLevel.Warn, ConsoleColor.Magenta },
            { LogLevel.Info, ConsoleColor.White },
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Trace, ConsoleColor.DarkGray },
        };

        public ConsoleOutLogger(string logName, LogLevel logLevel, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat, bool useColor) :
            base(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat) => UseColor = useColor;

        private bool UseColor { get; }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            var sb = new StringBuilder();
            FormatOutput(sb, level, message, exception);
            if (UseColor && colors.TryGetValue(level, out var color))
            {
                var originalColor = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = color;
                    Console.Out.WriteLine(sb.ToString());
                }
                finally
                {
                    Console.ForegroundColor = originalColor;
                }
            }
            else Console.Out.WriteLine(sb.ToString());
        }
    }
}
