using System;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using LazyStackAwsSettings;

namespace LazyStackGetAwsSettings
{
    public interface ILogger
    {
        void Info(string message);
        Task InfoAsync(string message);
        void Error(Exception ex, string message);
        Task ErrorAsync(Exception ex, string message);
    }

    public class Logger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public async Task InfoAsync(string message)
        {
            await Task.Delay(0);
            Console.WriteLine(message);
        }

        public void Error(Exception ex, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
        }

        public async Task ErrorAsync(Exception ex, string message)
        {
            await Task.Delay(0);
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
        }
    }

    class Program
    {
        [Verb("settings", HelpText = "Generate AWS settings files")]
        public class SettingsOptions
        {
            [Value(0, Required = true, HelpText = "Name of Stack")]
            public string StackName { get; set; }

            [Value(1, Required = false, HelpText = "FilePath")]
            public string OutputFilePath { get; set; }

            [Option('n', "profilename", Required = false, HelpText = "Specify AWS profile", Default = (string)"default")]
            public string ProfileName { get; set; }

            [Option('m', "methodmap", Required = false, HelpText = "Generate MethodMap class insted of AwsSettings")]
            public bool MethodMap { get; set; }
        }


        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<SettingsOptions>(args)
                .MapResult(
                    (SettingsOptions opts) => RunSettings(opts),
                    errs => 1
                 );
        }

        /// <summary>
        /// Generate AWS Settings file(s)
        /// </summary>
        /// <param name="settingsOptions"></param>
        /// <returns></returns>
        public static int RunSettings(SettingsOptions settingsOptions)
        { 
            var logger = new Logger();
            try
            {
                if (string.IsNullOrEmpty(settingsOptions.StackName))
                    throw new Exception($"Error: no StackName provided");

                var json = (settingsOptions.MethodMap)
                    ? AwsConfig.GenerateMethodMapJsonAsync(
                        settingsOptions.ProfileName,
                        settingsOptions.StackName).GetAwaiter().GetResult()
                    : AwsConfig.GenerateSettingsJsonAsync(
                        settingsOptions.ProfileName,
                        settingsOptions.StackName).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(settingsOptions.OutputFilePath))
                    Console.Write(json);
                else
                    File.WriteAllText(settingsOptions.OutputFilePath, json);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                return -1;
            }
            return 1;
        }
    }
}
