using System;
using N = NLog;
using A = AddUp.AnyLog;
using AddUp.AnyLog;
using System.Diagnostics;

namespace PerfTestsApp
{
    internal sealed class Program
    {
        private static readonly N.ILogger mainLog = N.LogManager.GetCurrentClassLogger();
        private static void Main() => new Program().Run();

        private void Run()
        {
            var count = 10;
            mainLog.Info($"WARMUP: count = {count}");
            RunTest(0, count);

            for (var i = 1; i < 5; i++)
            {
                count = (int)Math.Pow(10, i);
                mainLog.Info($"REAL RUN #{i}: count = {count}");
                RunTest(i, count);
            }
        }

        private void RunTest(int index, int count)
        {
            var sw = new Stopwatch();
            sw.Start();

            var nlogLog = N.LogManager.GetLogger($"Perfs.{index}");
            for (var i = 0; i < count; i++)
                LogWithNLog(nlogLog, $"NLog: Log #{i}");
            mainLog.Info($"NLog   - Elapsed = {sw.Elapsed}");

            sw.Restart();

            var anyLog = LogManager.GetLogger($"Perfs.{index}");
            for (var i = 0; i < count; i++)
                LogWithAnyLog(anyLog, $"AnyLog: Log #{i}");
            mainLog.Info($"AnyLog - Elapsed = {sw.Elapsed}");

            sw.Stop();
        }

        private void LogWithNLog(N.ILogger log, string message, bool withException = true)
        {
            log.Fatal(message);
            log.Error(message);
            log.Warn(message);
            log.Info(message);
            log.Debug(message);
            log.Trace(message);
            if (withException)
                log.Error(new ApplicationException("Test"), message);
        }

        private void LogWithAnyLog(ILog log, string message, bool withException = true)
        {
            log.Fatal(message);
            log.Error(message);
            log.Warn(message);
            log.Info(message);
            log.Debug(message);
            log.Trace(message);
            if (withException)
                log.Error(message, new ApplicationException("Test"));
        }
    }
}
