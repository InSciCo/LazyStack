﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using LazyStack;
using VSLangProj;
using System.Windows;

namespace LazyStackVsExt
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LazyStack_Generate_Projects
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("74dda08f-2bee-4ed4-97b0-1405b9fbbb16");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly DTE dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyStack_Generate_Projects"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LazyStack_Generate_Projects(AsyncPackage package, DTE dte, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LazyStack_Generate_Projects Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in LazyStack___Generate_Projects's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            Instance = new LazyStack_Generate_Projects(package, dte, commandService);

        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Do the work in this handler async
            this.package.JoinableTaskFactory.RunAsync((Func<Task>)async delegate
            {
                var solutionFullName = dte?.Solution?.FullName;
                if (string.IsNullOrEmpty(solutionFullName))
                {
                    MessageBox.Show("Sorry - no solution is open.");
                    return;
                }

                ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(LazyStackLogToolWindow), 0, true, this.package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                    throw new NotSupportedException("Cannot create tool window");

                var userControl = window.Content as LazyStackLogToolWindowControl;
                userControl.LogEntries.Clear();

                // Progress class
                // Any handler provided to the constructor or event handlers registered with the 
                // ProgressChanged event are invoked through a SynchronizationContext instance captured 
                // when the instance is constructed. If there is no current SynchronizationContext 
                // at the time of construction, the callbacks will be invoked on the ThreadPool.
                // Practical Effect; Progress allows us to avoid wiring up events to handle logging entries
                // made by CPU bound tasks executed with await Task.Run(...)
                var progress = new Progress<LogEntry>(l => userControl.LogEntries.Add(l));
                var logger = new Logger(progress); // ie Logger.Info(msg) calls progress.Report(logEntry).

                try
                {
                    var solutionRootFolderPath = Path.GetDirectoryName(solutionFullName);
                    var solutionModel = new SolutionModel(solutionFullName, logger);

                    // Process the API yaml files
                    await solutionModel.ProcessOpenApiAsync();

                    // Write the new serverless.template file
                    await solutionModel.WriteSAMAsync();

                    // Create / Update the Projects
                    var processProjects = new ProcessProjects(solutionModel, logger);
                    await processProjects.RunAsync();

                    // Avoid unnecessary warnings on access to dte etc.
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                    var solution = dte.Solution as Solution4;
                    var appName = solutionModel.AppName;

                    // Solution Items folder
                    var projectPath = new List<string> { "Solution Items" };
                    Project solutionItemsProject = GetProject(projectPath);
                    if (solutionItemsProject == null)
                        solutionItemsProject = solution.AddSolutionFolder("Solution Items");

                    AddFileToProject(solutionItemsProject, Path.Combine(solutionRootFolderPath, $"{appName}.yaml"));
                    AddFileToProject(solutionItemsProject, Path.Combine(solutionRootFolderPath, "LazyStack.yaml"));
                    AddFileToProject(solutionItemsProject, Path.Combine(solutionRootFolderPath, "SAM_Review.yaml"));

                    // Add root level solutionModel projects to root folder 
                    foreach (var kvp in solutionModel.Projects)
                        if (string.IsNullOrEmpty(kvp.Value.SolutionFolder))
                        {
                            bool found = false;
                            foreach (Project project in solution.Projects)
                                if (found = project.Name.Equals(kvp.Key))
                                    break;
                            if (!found)
                            {
                                await logger.InfoAsync($"Loading project {kvp.Key} from  {kvp.Value.Path}");
                                solution.AddFromFile(kvp.Value.Path, Exclusive: false);
                            }
                        }


                    // Lambdas folder - add lambda projects under Lambdas SolutionFolder
                    var folderName = "Lambdas";
                    projectPath = new List<string> { folderName };
                    Project lambdasProject = GetProject(projectPath);
                    if (lambdasProject == null)
                        lambdasProject = solution.AddSolutionFolder(folderName);
                    foreach (var kvp in solutionModel.Projects)
                        if (kvp.Value.SolutionFolder.Equals(folderName))
                        {
                            bool found = false;
                            foreach (ProjectItem projectItem in lambdasProject.ProjectItems)
                                if (projectItem.FileCount > 0)
                                    if (found = projectItem.Name.Equals(kvp.Key))
                                        break;
                            if (!found)
                            {
                                await logger.InfoAsync($"Loading Lambdas project {kvp.Key} from  {kvp.Value.Path}");
                                ((SolutionFolder)lambdasProject.Object).AddFromFile(kvp.Value.Path);
                            }
                        }

                    // Controllers folder -- add Controller's projects under Controllers SolutionFolder
                    folderName = "Controllers";
                    projectPath = new List<string> { folderName };
                    Project controllersProject = GetProject(projectPath);
                    if (controllersProject == null)
                        controllersProject = solution.AddSolutionFolder(folderName);
                    foreach (var kvp in solutionModel.Projects)
                        if (kvp.Value.SolutionFolder.Equals(folderName))
                        {
                            bool found = false;
                            foreach (ProjectItem projectItem in controllersProject.ProjectItems)
                                if (projectItem.FileCount > 0)
                                    if (found = projectItem.Name.Equals(kvp.Key))
                                        break;
                            if (!found)
                            {
                                await logger.InfoAsync($"Loading Controllers project {kvp.Key} from  {kvp.Value.Path}");

                                ((SolutionFolder)controllersProject.Object).AddFromFile(kvp.Value.Path);
                            }
                        }


                    // Stacks folder
                    folderName = "Stacks";
                    projectPath = new List<string> { folderName };
                    Project stacksProject = GetProject(projectPath);
                    if (stacksProject == null)
                    {
                        stacksProject = solution.AddSolutionFolder(folderName);
                    }

                    // Add folders and files for each Stacks environment
                    foreach (var env in solutionModel.Environments)
                    {
                        var envProjectPath = new List<string> { folderName, env.Key };
                        var envProject = GetProject(envProjectPath);
                        if (envProject == null)
                            envProject = ((SolutionFolder)stacksProject.Object).AddSolutionFolder(env.Key);
                        // AddFromDirectory not implemented for SolutionFolder 
                        //envProject.ProjectItems.AddFromDirectory(Path.Combine(solutionRootFolderPath, folderName, env.Key));
                        var files = Directory.GetFiles(Path.Combine(solutionRootFolderPath, folderName, env.Key));
                        foreach (var file in files)
                            AddFileToProject(envProject, file);
                    }

                    await logger.InfoAsync("LazyStack processing complete");

                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync(ex, "LazyStack Encountered an Error");
                }

            });

        }


        //Notes on Project, Projects, ProjectItems, SolutionFolders etc
        // Solution
        //   Projects -- Docs say all project are shown, however, only first level projects are!
        //     Project
        //       ProjectItems
        //         ProjectItem
        //           if projectItem.SubProject then use SubProject to drill down -- Note SolutionFolders are SubProjects!
        //           SubProject is not null even if there are no items in the SubProject
        // SolutionFolder is a type of Project eg. project.Kind == ProjectKinds.vsProjectKindSolutionFolder
        // solutionFolder = (SolutionFolder)project.Object
        // solutionFolder.AddFromFile(..)
        // project.ProjectItems.AddFromFile(..)
        // projectItem.FileNames[] has an ordinal starting at 1 instead of 0


        // Find a project based on supplied path
        private Project GetProject(List<string> path, Project subProject = null, int level = 0)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (level == 0)
            {
                var solution = dte?.Solution as Solution4;
                var projects = solution?.Projects;
                // Got through top level - recurse into project if found
                foreach (Project project in projects)
                    if (project.Name.Equals(path[level]))
                        return (level == path.Count - 1)
                            ? project
                            : GetProject(path, project, ++level);
            }
            else
            {
                foreach (ProjectItem projectItem in subProject.ProjectItems)
                    if (projectItem.Name.Equals(path[level]))
                        if (projectItem.SubProject != null) // also pick's up SolutionFolders
                            return (level == path.Count - 1)
                                ? (Project)projectItem.Object
                                : GetProject(path, (Project)projectItem, ++level);
            }
            return null;
        }

        private void AddFileToProject(Project project, string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project == null)
                throw new ArgumentNullException("Error: project is null");

            if (filePath == null)
                throw new ArgumentNullException("Error: filePath is null");

            // This is not an error - we only add the file if it exists
            if (!File.Exists(filePath))
                return;
            
            // just return if the file is already referenced in project
            foreach (ProjectItem projectItem in project.ProjectItems)
                if(projectItem.FileCount == 1)
                    if (projectItem.FileNames[1].Equals(filePath)) // note bizarre ordinal 1 !
                        return;

            project.ProjectItems.AddFromFile(filePath);

        }

    }
}