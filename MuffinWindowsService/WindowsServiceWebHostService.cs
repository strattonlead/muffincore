using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace MuffinWindowsService
{
    [DesignerCategory("Code")]
    public abstract class WindowsServiceWebHostService : WebHostService
    {
        protected readonly ILogger Logger;
        private uint fPreviousExecutionState;

        public WindowsServiceWebHostService(IWebHost host) : base(host)
        {
            Logger = host.Services.GetService<ILogger<WindowsServiceWebHostService>>();
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(
            NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            if (fPreviousExecutionState == 0)
            {
                Console.WriteLine($"SetThreadExecutionState failed. code: {fPreviousExecutionState}");
                Logger?.LogError($"SetThreadExecutionState failed. code: {fPreviousExecutionState}");
            }
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            if (NativeMethods.SetThreadExecutionState(fPreviousExecutionState) == 0)
            {

            }
        }
    }

    internal class DefaultCustomWebHostService : WindowsServiceWebHostService
    {
        public DefaultCustomWebHostService(IWebHost host)
            : base(host) { }

        protected override void OnStarting(string[] args)
        {
            Logger?.LogTrace("OnStarting method called.");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            Logger?.LogTrace("OnStarted method called.");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            Logger?.LogTrace("OnStopping method called.");
            base.OnStopping();
        }
    }

    public static class WebHostServiceExtensions
    {
        public static void RunAsWindowsService(string[] args, Func<string[], IWebHostBuilder> hostBuilder)
        {
            RunAsWindowsService(args, null, hostBuilder);
        }

        public static void RunAsWindowsService(string[] args, Action<IWebHost> hostBuilt, Func<string[], IWebHostBuilder> hostBuilder)
        {
            try
            {
                var isService = !(Debugger.IsAttached || args.Contains("--console"));

                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                    Directory.SetCurrentDirectory(pathToContentRoot);
                }

                var builder = hostBuilder(args.Where(arg => arg != "--console")
                    .ToArray());

                var host = builder.Build();

                hostBuilt?.Invoke(host);

                if (isService)
                {
                    Console.WriteLine("Run as service!");
                    host.RunAsCustomService();
                }
                else
                {
                    Console.WriteLine("Run as default web host!");
                    host.Run();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                try
                {
                    if (!File.Exists("startuperror.log"))
                    {
                        File.Create("startuperror.log").Close();
                    }
                    File.AppendAllText("startuperror.log", "\n" + e.ToString());
                }
                catch { }
            }
        }

        private static void RunAsCustomService(this IWebHost host)
        {
            host.RunAsCustomService<DefaultCustomWebHostService>();
        }

        private static void RunAsCustomService<T>(this IWebHost host)
            where T : WindowsServiceWebHostService
        {
            var webHostService = (T)Activator.CreateInstance(typeof(T), new object[] { host });
            Console.WriteLine($"RunAsCustomService {typeof(T).Name}");
            ServiceBase.Run(webHostService);
        }
    }

    internal static class NativeMethods
    {
        /// <summary>
        /// Import SetThreadExecutionState Win32 API and necessary flags.
        /// https://stackoverflow.com/a/6302309/633945
        /// </summary>
        /// <param name="esFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
    }
}
