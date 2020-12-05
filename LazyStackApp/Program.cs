using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using LazyStack;
using Microsoft.DotNet.Cli.Sln.Internal.Lz;
using Microsoft.DotNet.Tools.Common.Lz;
//using Microsoft.Build.Locator;

namespace LazyStackApp
{
    public class Logger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(Exception ex, string message )
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

        static void Main(string[] args)
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
            solutionModel.ProcessOpenApi();

            // Create / Update the Projects
            var processProjects = new ProcessProjects(solutionModel, logger);
            processProjects.Run();

            // Update the Solution File
            var slnFile = SlnFile.Read(Path.Combine(solutionRootFolderPath, $"{solutionModel.AppName}.sln"));

            foreach(KeyValuePair<string,ProjectInfo> proj in solutionModel.Projects)
            {
                var projName = proj.Key;
                var projInfo = proj.Value;
                if(!slnFile.ProjectExists(projName))
                {
                    logger.Info($"Adding Project {projName} to solution");
                    slnFile.AddProject(projInfo.Path, projInfo.RelativePath, solutionRootFolderPath);
                }
            }

            // Add AppName.yaml file to SolutionItems folder if it exists
            var itemName = $"{solutionModel.AppName}.yaml";
            if (File.Exists(Path.Combine(solutionRootFolderPath, itemName)))
                AddSolutionItemFolderFile(slnFile, itemName, logger);

            // Add template.yaml file to SolutionItems folder if it exists
            itemName = "template.yaml";
            if (File.Exists(Path.Combine(solutionRootFolderPath, itemName)))
                AddSolutionItemFolderFile(slnFile, itemName, logger);

            // Add serverless.template file to SolutionItems folder if it exists
            itemName = "serverless.template";
            if (File.Exists(Path.Combine(solutionRootFolderPath, itemName)))
                AddSolutionItemFolderFile(slnFile, itemName, logger);

            // Add LazyStack.yaml file to SolutionItems folder if it exists
            itemName = "LazyStack.yaml";
            if (File.Exists(Path.Combine(solutionRootFolderPath, itemName)))
                AddSolutionItemFolderFile(slnFile, itemName, logger);

            slnFile.Write();
        }

        public static void AddSolutionItemFolderFile(SlnFile slnFile, string fileName, ILogger logger)
        {
            if (!slnFile.ProjectExists("SolutionItems"))
            {
                logger.Info($"Adding SolutionItems folder");
                slnFile.AddSolutionFolder("SolutionItems");
            }

            var slnProject = slnFile.GetProjectByName("SolutionItems");

            var slnSectionCollection = slnProject.Sections;

            var slnSection = slnSectionCollection.GetOrCreateSection("SolutionItems", SlnSectionType.PreProcess);

            var slnProperties = slnSection.Properties;

            slnProperties.SetValue(fileName, fileName);

            logger.Info($"Adding {fileName} to SolutionItems folder");
        }

    }
}
