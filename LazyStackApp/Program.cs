using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using CommandLine;
using LazyStack;
using Microsoft.DotNet;
using Microsoft.DotNet.InternalAbstractions;

//using Microsoft.Build.Locator;

namespace LazyStackApp
{
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
        // https://github.com/commandlineparser/commandline
        [Verb("projects", isDefault: true, HelpText = "Generate/Update project files")]
        public class ProjectsOptions
        {
            [Option('s', "solutionfile", Required = false, HelpText = "Specify solution file")]
            public string SolutionFilePath { get; set; }

            [Option('e', "env", Required = false, HelpText = "Specify environment")]
            public string Environment { get; set; } = "Dev";
        }

        [Verb("settings", HelpText = "Generate/Update AWS settings files")]
        public class SettingsOptions
        {
            [Option('s', "solutionfile", Required = false, HelpText = "Specify solution file")]
            public string SolutionFilePath { get; set; }

            [Option('e', "env", Required = false, HelpText = "Specify environment")]
            public string Environment { get; set; } = "Dev";
        }

        // todo: should be "static Task<int> Main(...) so everything can be async. However, the 
        // CommandLine library argues. Figure this out at some point but this is not urgent.
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<ProjectsOptions, SettingsOptions>(args)
                .MapResult(
                    (ProjectsOptions opts) => RunProjects(opts),
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
            var solutionFilePath = settingsOptions.SolutionFilePath;

            var solutionRootFolderPath = Path.GetDirectoryName(solutionFilePath); // may be empty
            var appName = Path.GetFileNameWithoutExtension(solutionFilePath); // may be empty

            // use current directory if no directory was specified           
            if (string.IsNullOrEmpty(solutionRootFolderPath))
                solutionRootFolderPath = Directory.GetCurrentDirectory();

            // find sln file if none was specified
            if (string.IsNullOrEmpty(appName))
            {
                // Find Visual Studio solution file
                var solFiles = Directory.GetFiles(solutionRootFolderPath, "*.sln");
                if (solFiles.Length > 1)
                    throw new System.Exception("More than one .sln file in folder");

                if (solFiles.Length == 0)
                    throw new System.Exception("Solution file not found");

                appName = Path.GetFileNameWithoutExtension(solFiles[0]);
                solutionFilePath = Path.Combine(solutionRootFolderPath, $"{appName}.sln");
            }


            try
            {
                AwsConfig.GenerateSettingsFileAsync(solutionRootFolderPath,settingsOptions.Environment, logger).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                return -1;
            }
            return 1;
        }

        /// <summary>
        /// Generate/Update projects
        /// </summary>
        /// <param name="projectsOptions"></param>
        /// <returns></returns>
        public static int RunProjects(ProjectsOptions projectsOptions)
        {
            var logger = new Logger();
            try
            { 
                var solutionModel = new SolutionModel(projectsOptions.SolutionFilePath, logger);

                // Process the API yaml files
                solutionModel.ProcessOpenApiAsync().GetAwaiter().GetResult();

                solutionModel.WriteSAMAsync().GetAwaiter().GetResult();

                solutionModel.WriteSolutionModelAwsSettings().GetAwaiter().GetResult();

                // Create / Update the Projects
                var processProjects = new ProcessProjects(solutionModel, logger);
                processProjects.RunAsync().GetAwaiter().GetResult();

                // Get current projects using "dotnet sln <slnFilePath> list
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = "dotnet";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.Arguments = $" sln {solutionModel.SolutionFilePath} list";
                using var process = Process.Start(startInfo);
                var existingProjects = process.StandardOutput.ReadToEnd();
                Console.WriteLine($"Existing Projects\n{existingProjects}");

                var projectsToAdd = string.Empty;
                foreach (KeyValuePair<string, ProjectInfo> proj in solutionModel.Projects)
                    if (!existingProjects.Contains($"{proj.Value.RelativePath}"))
                    {
                        logger.Info($"Adding Project {proj.Value.RelativePath} to solution");
                        projectsToAdd += $" {proj.Value.RelativePath}";
                    }
                if (!string.IsNullOrEmpty(projectsToAdd))
                {
                    startInfo.Arguments = $"sln {solutionModel.SolutionFilePath} add {projectsToAdd}";
                    using var addProcess = Process.Start(startInfo);
                    var addProjectOutput = addProcess.StandardOutput.ReadToEnd();
                    Console.WriteLine($"{addProjectOutput}");
                }

                    // Note!
                    // Unlike the VisualStudio processing, we do not have the ability to 
                    // create a Solution Items folder and add items to it when using 
                    // the dotnet CLI. If someone is working with the CLI, it is not clear
                    // if a Solution Items folder would add any value. OTOH, if a solution
                    // generated with Visual Studio and which is later processed using 
                    // the dotnet CLI, nothing bad happens. So, no harm no foul.
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
