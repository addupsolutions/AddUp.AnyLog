using AddUp.CommonLogging;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof(Program));

            var netfx =
#if NETFRAMEWORK
                ".NET Framework";
#else
                ".NET Core";
#endif
            log.Info($"Message from AddUp.CommonLogging+NLog Test App ({netfx})");
            App.Run();
        }
    }
}
