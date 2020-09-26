using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AddUp.AnyLog
{
    [ExcludeFromCodeCoverage]
    internal sealed class DefaultAdapter : ILoggingFrameworkAdapter
    {
        private const bool shouldLogToConsole = true;

        public DefaultAdapter(LoggingFrameworkDescriptor descriptor) => Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        public LoggingFrameworkDescriptor Descriptor { get; }

        public void Log(string loggerName, LogLevel level, string message, Exception exception)
        {
            const int maxLoggerNameLength = 10;

            string formatLevel()
            {
                switch (level)
                {
                    case LogLevel.Fatal: return "F";
                    case LogLevel.Error: return "E";
                    case LogLevel.Warn: return "W";
                    case LogLevel.Info: return "I";
                    case LogLevel.Debug: return "D";
                    case LogLevel.Trace: return "T";
                }

                return "X";
            }

            var builder = new StringBuilder();
            _ = builder.Append(formatLevel());
            _ = builder.Append("|");

            var truncatedLoggerName = TruncateLoggerName(loggerName, maxLoggerNameLength);
            _ = builder.Append($"{truncatedLoggerName}|");

            _ = builder.Append(message ?? "");

            if (exception != null)
            {
                if (!string.IsNullOrEmpty(message))
                    _ = builder.AppendLine();
                _ = builder.Append(exception.ToString());
            }

            var text = builder.ToString();
            if (shouldLogToConsole) Console.WriteLine(text);

            // In all cases dump to VS output
            Debug.WriteLine(builder.ToString());
        }

        private static string TruncateLoggerName(string name, int max)
        {
            if (string.IsNullOrEmpty(name)) return new string(' ', max);

            return name.Length > max ?
                name.Substring(name.Length - max) :
                name.PadLeft(max);
        }
    }
}
