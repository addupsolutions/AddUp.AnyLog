using MyLib;

namespace TestApp
{
    internal static class App
    {
        public static void Run()
        {
            var api = new MyApi();
            api.DoSomething("Test", false);
            api.DoSomething("Test", true);
        }
    }
}
