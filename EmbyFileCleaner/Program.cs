namespace EmbyFileCleaner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Model.Json;
    using Mono.Options;
    using Newtonsoft.Json;

    public class Program
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            Logger.Info($"Emby File Cleaner v{GetAssemblyVersion()}");
            if (GetConfigPath(args, out var configPath))
            {
                var config = GetConfig(configPath);
                var cleaner = new Cleaner(config);
                cleaner.Run();
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        public static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private static bool GetConfigPath(string[] args, out string configPath)
        {
            string path = "./config.json";
            var p = new OptionSet
            {
                {
                    "cp|configPath=", "the {PATH} to config path.",
                    v => path = v
                },
                {
                    "h|help", "show this message and exit",
                    v => { }
                }
            };

            try
            {
                p.Parse(args);
                configPath = path;
                return true;
            }
            catch (OptionException e)
            {
                Logger.Info($"{e.Message}");
                configPath = null;
                return false;
            }
        }

        private static Config GetConfig(string configPath)
        {
            var file = File.OpenText(configPath);
            var configString = file.ReadToEnd();
            return JsonConvert.DeserializeObject<Config>(configString);
        }
    }
}