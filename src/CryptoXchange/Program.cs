using System;
using System.Diagnostics;
using System.IO;
using CryptoXchange.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace CryptoXchange
{
    public class Program
    {
        private static ILogger _logger;

        public static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

                ConfigureLogging();

                var host = BuildWebHost(args);
                Logo();
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.WriteLine("CryptoXchange cannot start.");
            }
            Shutdown();
            Process.GetCurrentProcess().CloseMainWindow();
            Process.GetCurrentProcess().Close();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

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

        private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_logger != null)
            {
                _logger.Error(e.ExceptionObject);
                LogManager.Flush(TimeSpan.Zero);
            }

            Console.WriteLine("** AppDomain unhandled exception: {0}", e.ExceptionObject);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _logger?.Info(() => "SIGTERM received. Exiting.");
            Console.WriteLine("SIGTERM received. Exiting.");
        }

        private static void Shutdown()
        {
            _logger?.Info(() => "Shutdown ...");
            Console.WriteLine("Shutdown...");
        }

        private static void ConfigureLogging()
        {
            var config = new ClusterLoggingConfig
            {
                Level = "info",
                EnableConsoleLog = true,
                EnableConsoleColors = true,
                LogFile ="log.txt",
                LogBaseDirectory = ""
            };
            var loggingConfig = new LoggingConfiguration();

            if (config != null)
            {
                // parse level
                var level = !string.IsNullOrEmpty(config.Level)
                    ? LogLevel.FromString(config.Level)
                    : LogLevel.Info;

                var layout = "[${longdate}] [${level:format=FirstCharacter:uppercase=true}] [${logger:shortName=true}] ${message} ${exception:format=ToString,StackTrace}";

                if (config.EnableConsoleLog)
                {
                    if (config.EnableConsoleColors)
                    {
                        var target = new ColoredConsoleTarget("console")
                        {
                            Layout = layout
                        };

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Trace"),
                            ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.NoChange));

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Debug"),
                            ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Info"),
                            ConsoleOutputColor.White, ConsoleOutputColor.NoChange));

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Warn"),
                            ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Error"),
                            ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));

                        target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                            ConditionParser.ParseExpression("level == LogLevel.Fatal"),
                            ConsoleOutputColor.DarkRed, ConsoleOutputColor.White));

                        loggingConfig.AddTarget(target);
                        loggingConfig.AddRule(level, LogLevel.Fatal, target);
                    }

                    else
                    {
                        var target = new ConsoleTarget("console")
                        {
                            Layout = layout
                        };

                        loggingConfig.AddTarget(target);
                        loggingConfig.AddRule(level, LogLevel.Fatal, target);
                    }
                }

                if (!string.IsNullOrEmpty(config.LogFile))
                {
                    var target = new FileTarget("file")
                    {
                        FileName = GetLogPath(config, config.LogFile),
                        FileNameKind = FilePathKind.Unknown,
                        Layout = layout
                    };

                    loggingConfig.AddTarget(target);
                    loggingConfig.AddRule(level, LogLevel.Fatal, target);
                }
            }

            LogManager.Configuration = loggingConfig;

            _logger = LogManager.GetCurrentClassLogger();
        }

        private static Layout GetLogPath(ClusterLoggingConfig config, string name)
        {
            if (string.IsNullOrEmpty(config.LogBaseDirectory))
                return name;

            return Path.Combine(config.LogBaseDirectory, name);
        }

    }
}
