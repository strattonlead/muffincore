//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace Muffin.AspNetCore.Ngrok
//{
//    public class NgrokService : BackgroundService
//    {
//        #region Properties

//        private readonly IHostingEnvironment HostingEnvironment;
//        private readonly IHostApplicationLifetime HostApplicationLifetime;

//        #endregion

//        #region Constructor

//        public NgrokService(IServiceProvider serviceProvider)
//        {
//            HostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
//            HostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
//        }

//        #endregion

//        #region IHostedService

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            await Task.Run(async () =>
//            {
//                string ngrokWorkPath = null;
//                var localNgrokPath = GetFullPathFromWindows("ngrok.exe");
//                if (localNgrokPath != null && File.Exists(localNgrokPath))
//                {
//                    ngrokWorkPath = localNgrokPath;
//                }
//                else
//                {
//                    var ngrokPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "ngrok.exe");
//                    if (!File.Exists(ngrokPath))
//                    {
//                        using (var stream = typeof(NgrokService).Assembly.GetManifestResourceStream("Muffin.AspNetCore.Ngrok.ngrok.exe"))
//                        using (var fileStream = File.OpenWrite(ngrokPath))
//                        {
//                            await stream.CopyToAsync(fileStream);
//                        }
//                    }
//                    ngrokWorkPath = ngrokPath;
//                }

//                while (!HostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
//                {
//                    // prüfen ob der prozess läuft
//                }
//            });
//        }

//        #endregion

//        #region Helpers

//        /// <summary>
//        /// Gets the full path of the given executable filename as if the user had entered this
//        /// executable in a shell. So, for example, the Windows PATH environment variable will
//        /// be examined. If the filename can't be found by Windows, null is returned.</summary>
//        /// <param name="exeName"></param>
//        /// <returns>The full path if successful, or null otherwise.</returns>
//        public static string GetFullPathFromWindows(string exeName)
//        {
//            if (exeName.Length >= MAX_PATH)
//                throw new ArgumentException($"The executable name '{exeName}' must have less than {MAX_PATH} characters.",
//                    nameof(exeName));

//            var sb = new StringBuilder(exeName, MAX_PATH);
//            return PathFindOnPath(sb, null) ? sb.ToString() : null;
//        }

//        // https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
//        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
//        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
//        static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);

//        // from MAPIWIN.h :
//        private const int MAX_PATH = 260;

//        #endregion
//    }

//    public class NgrokServiceOptions
//    {
//        public string NgrokApiKey { get; set; }
//        public string NgrokPath { get; set; }
//    }

//    public class NgrokServiceOptionsBuilder
//    {

//    }

//    public static class NgrokServiceExtensions
//    {
//        public static void AddNgrokService(this IServiceCollection services)
//        {
//            services.AddSingleton<NgrokService>();
//        }
//    }
//}