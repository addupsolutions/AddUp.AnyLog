using System;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif
            Console.WriteLine($"Message from No Logging Framework Test App ({netfx})");
            App.Run();
        }
    }
}
