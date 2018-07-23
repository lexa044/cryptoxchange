using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using System.Reflection;

using log4net;
using log4net.Config;

namespace CryptoXchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var host = BuildWebHost(args);
            Logo();
            host.Run();
            Process.GetCurrentProcess().CloseMainWindow();
            Process.GetCurrentProcess().Close();
        }

        private static void Logo()
        {
            Console.WriteLine($@"
    _____                  _      __   __    _                            
  / ____|                | |     \ \ / /   | |                           
 | |     _ __ _   _ _ __ | |_ ___ \ V / ___| |__   __ _ _ __   __ _  ___ 
 | |    | '__| | | | '_ \| __/ _ \ > < / __| '_ \ / _` | '_ \ / _` |/ _ \
 | |____| |  | |_| | |_) | || (_) / . \ (__| | | | (_| | | | | (_| |  __/
  \_____|_|   \__, | .__/ \__\___/_/ \_\___|_| |_|\__,_|_| |_|\__, |\___|
               __/ | |                                         __/ |     
              |___/|_|                                        |___/      
");
            Console.WriteLine($"https://github.com/lexa044/cryptoxchange\n");
            Console.WriteLine();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
