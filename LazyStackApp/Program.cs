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
            Console.WriteLine(message);
        }

        public void Error(Exception ex, string message )
        {
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
        }

        public async Task ErrorAsync(Exception ex, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
        }

    }

    class Program
    {
        // https://github.com/commandlineparser/commandline
        public class Options
        {
            [Option('s', "solutionpath", Required = false, HelpText = "Specify path to solution root folder")]
            public string SolutionPath { get; set; }
        }

        static async Task Main(string[] args)
        {
            var solutionRootFolderPath = Directory.GetCurrentDirectory();  // default to current working directory

            Parser.Default.ParseArguments<Options>(args)
                 .WithParsed<Options>(o =>
                 {
                     if (!string.IsNullOrEmpty(o.SolutionPath))
                     {
                         solutionRootFolderPath = o.SolutionPath;
                         if (!Directory.Exists(solutionRootFolderPath))
                             throw new System.Exception("Specified solution root folder does not exist");
                     }
                 });

            var logger = new Logger();

            //MSBuildLocator.RegisterDefaults();

            var solutionModel = new SolutionModel(solutionRootFolderPath, logger);

            // Process the API yaml files
            await solutionModel.ProcessOpenApiAsync();

            // Create / Update the Projects
            var processProjects = new ProcessProjects(solutionModel, logger);
            await processProjects.RunAsync();

            // Update the Solution File

            var slnFilePath = Path.Combine(solutionRootFolderPath, $"{solutionModel.AppName}.sln");

            // Get current projects using "dotnet sln <slnFilePath> list
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "dotnet";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = $" sln {slnFilePath} list";
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
                startInfo.Arguments = $"sln {slnFilePath} add {projectsToAdd}";
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
    }
}
