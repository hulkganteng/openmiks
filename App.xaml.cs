using System.Windows;
using VMixClone.Tests;

namespace VMixClone
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0 && (e.Args[0] == "--test" || e.Args[0] == "-t"))
            {
                try
                {
                    ProductionEngineTests.RunAll();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[FAIL] Test failure: {ex}");
                    Environment.Exit(1);
                }
                return;
            }

            base.OnStartup(e);
        }
    }
}
