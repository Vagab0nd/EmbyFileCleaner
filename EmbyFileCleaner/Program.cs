using System;
using System.Diagnostics;
using System.IO;
using EmbyFileCleaner.Model.Json;
using Mono.Options;
using Newtonsoft.Json;

namespace EmbyFileCleaner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Emby File Cleaner v{GetAssemblyVersion()}");
            if (args.Length > 0 && GetConfigPath(args, out string configPath))
            {
                var config = GetConfig(configPath);
                var cleaner = new Cleaner(config);
                cleaner.Run();
            }
            Console.ReadKey();
        }

        private static string GetAssemblyVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private static bool GetConfigPath(string[] args, out string configPath)
        {
            string path = null;
            bool show_help = false;
            var p = new OptionSet() {
                {
                    "cp|configPath=", "the {PATH} to config path.",
                    v => path = v
                },
                {
                    "h|help",  "show this message and exit",
                    v => show_help = v != null
                }
            };

            try
            {
                var extra = p.Parse(args);
                configPath = path;
                return true;
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `--help' option for more information.");
                configPath = null;
                return false;
            }
        }

        private static Config GetConfig(string configPath)
        {
            StreamReader file = File.OpenText(configPath);
            string configString = file.ReadToEnd();
            return JsonConvert.DeserializeObject<Config>(configString);
        }
    }
}
