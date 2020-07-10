using System;
using System.Reflection;
using System.ServiceProcess;

namespace UserProfileImageUpdater
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] servicesToRun = new ServiceBase[] {
                new UserProfileImageUpdaterService()
            };

            if (Environment.UserInteractive)
            {
                RunInteractive(servicesToRun);
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
        private static void RunInteractive(ServiceBase[] servicesToRun)
        {
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart",
            BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Starting {0}...", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.Write("{0} Started", service.ServiceName);
                Console.WriteLine("Press any key to stop the service {0}", service.ServiceName);
                Console.Read();
                Console.WriteLine();
            }
        }
    }
}
