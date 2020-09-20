using System;

namespace Scratch
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var names = new[]
            {
                null,
                "",
                "short",
                "0123456789",
                "Very.Long.Class.Name"
            };

            foreach (var name in names)
            {
                var n = TruncateLoggerName(name, 10);
                Console.WriteLine($"X|{n}|message");
            }
            
            Console.WriteLine("Hello World!");
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
