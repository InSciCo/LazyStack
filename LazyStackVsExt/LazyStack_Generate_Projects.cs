using System;
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
                if(string.IsNullOrEmpty(solutionFullName))
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
                    var solutionModel = new SolutionModel(solutionRootFolderPath, logger);

                    // Use Task.Run to do CPU bound work
                    // Process the API yaml files
                    await Task.Run(() => solutionModel.ProcessOpenApi());

                    // Create / Update the Projects
                    var processProjects = new ProcessProjects(solutionModel, logger);
                    await Task.Run(() => processProjects.Run());

                    // Avoid unnecessary warnings on access to dte etc.
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                    var solution = dte?.Solution as Solution4;

                    var appName = solutionModel.AppName;

                    // create LzStack solution folders if they do not exist
                    var solutionItemsFolder = CreateSolutionFolder("Solution Items");

                    // Add appName.yaml file to SolutionItems folder
                    if (File.Exists(Path.Combine(solutionRootFolderPath, $"{appName}.yaml")))
                        if (solution.FindProjectItem($"{appName}.yaml") == null)
                        {
                            logger.Info($"Adding {appName}.yaml to Solution Items folder");
                            solutionItemsFolder.Parent.ProjectItems.AddFromFile(
                                Path.Combine(solutionRootFolderPath, $"{appName}.yaml"));
                        }

                    // Add template.yaml file to SolutionItems folder if it exists
                    if (File.Exists(Path.Combine(solutionRootFolderPath, "template.yaml")))
                        if (solution.FindProjectItem("template.yaml") == null)
                        {
                            logger.Info($"Adding templateyaml to Solution Items folder");
                            solutionItemsFolder.Parent.ProjectItems.AddFromFile(
                                Path.Combine(solutionRootFolderPath, "template.yaml"));
                        }

                    // Add serverless.template file to SolutionItems folder if it exists
                    if (File.Exists(Path.Combine(solutionRootFolderPath, "serverless.template")))
                        if (solution.FindProjectItem("serverless.template") == null)
                        {
                            logger.Info($"Adding serverless.template to Solution Items folder");
                            solutionItemsFolder.Parent.ProjectItems.AddFromFile(
                                Path.Combine(solutionRootFolderPath, "serverless.template"));
                        }

                    // Add LazyStack.yaml file to SolutionItems folder if it exists
                    if (File.Exists(Path.Combine(solutionRootFolderPath, "LazyStack.yaml")))
                        if (solution.FindProjectItem("LazyStack.yaml") == null)
                        {
                            logger.Info($"Adding LazyStack.yaml to Solution Items folder");
                            solutionItemsFolder.Parent.ProjectItems.AddFromFile(
                                Path.Combine(solutionRootFolderPath, "LazyStack.yaml"));
                        }


                    // Get a dictionary of projects (keyed by subfolder/projectName where subfolder is used only on subprojects)
                    var projects = GetProjects();

                    // LzStack checks for the following projects and adds to sln if missing:
                    foreach(KeyValuePair<string,ProjectInfo> kvp in solutionModel.Projects)
                    {
                        var projName = kvp.Key;
                        var projInfo = kvp.Value;
                        if(!projects.ContainsKey(Path.Combine(projInfo.SolutionFolder,projName)))
                        {
                            if (string.IsNullOrEmpty(projInfo.SolutionFolder))
                            {   // Add project under solution root
                                logger.Info($"Adding {projName} to solution");
                                solution.AddFromFile(projInfo.Path,Exclusive: false);
                            }
                            else
                            {   // Add project under solution folder
                                logger.Info($"Adding {projName} to solution in solution folder {projInfo.SolutionFolder}");
                                var folder = CreateSolutionFolder(projInfo.SolutionFolder);
                                folder.AddFromFile(projInfo.Path);
                            }
                        }
                    }

                    logger.Info("LazyStack processing complete");

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "LazyStack Encountered an Error");
                }

            });

        }

        private Dictionary<string, Project> GetProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = dte?.Solution as Solution4;
            var folders = solution?.Projects;

            var projects = new Dictionary<string, Project>();

            // Got through top level - drill down when we find a solutions folder
            foreach (Project folder in folders)
                if (folder.Kind == PrjKind.prjKindCSharpProject)
                    projects.Add(folder.Name, folder);
                else
                    if (folder.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                        GetProjectsSub(projects, folder);
            return projects;
        }

        private void GetProjectsSub(Dictionary<string, Project> projects, Project folder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projectItems = folder.ProjectItems;
            var folderName = folder?.Name;

            foreach (ProjectItem projectItem in projectItems)
            {
                var subProject = projectItem?.SubProject;
                var subProjectName = subProject?.Name;
                if (subProject != null)
                    projects.Add(Path.Combine(folderName, subProjectName), subProject);
            }
        }


        private SolutionFolder CreateSolutionFolder(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solution = dte?.Solution as Solution4;
            var folders = dte?.Solution?.Projects;

            foreach (Project folder in folders)
                if (folder.Kind == ProjectKinds.vsProjectKindSolutionFolder && folder.Name.Equals(name))
                    return (SolutionFolder)folder.Object;

            try
            {
                var newFolder = solution.AddSolutionFolder(name);
                var isSolutionFolder = newFolder.Kind == ProjectKinds.vsProjectKindSolutionFolder;
                var returnFolder = (SolutionFolder)newFolder.Object;
                return returnFolder;
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
