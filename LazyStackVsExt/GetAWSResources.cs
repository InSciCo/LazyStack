using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft;

using LazyStack;
using Amazon;
using Amazon.Runtime.CredentialManagement;


namespace LazyStackVsExt
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GetAWSResources
    {

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4130;

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
        /// Initializes a new instance of the <see cref="GetAWSResources"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GetAWSResources(AsyncPackage package, DTE dte, OleMenuCommandService commandService)
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
        public static GetAWSResources Instance
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
            // Switch to the main thread - the call to AddCommand in GetAWSResources's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE;

            Instance = new GetAWSResources(package, dte, commandService);
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

            this.package.JoinableTaskFactory.RunAsync((Func<Task>)async delegate
           {


               var solutionFullName = dte?.Solution?.FullName;
               if (string.IsNullOrEmpty(solutionFullName))
               {
                   MessageBox.Show("Sorry - no solution is open.");
                   return;
               }

               var solutionFolderPath = Path.GetDirectoryName(solutionFullName);

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

               // Avoid unnecessary warnings on access to dte etc.
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
               await AwsConfig.GenerateSettingsFileAsync(solutionFolderPath, "Dev", logger);

               var solution = dte?.Solution as Solution4;
               // create LzStack solution folders if they do not exist
               var solutionItemsFolder = CreateSolutionFolder("Solution Items");

               // Add Dev.AwsSettings.json to solution folder
               var awsSettingsFileName = "Dev.AwsSettings.json";
               if (File.Exists(Path.Combine(solutionFolderPath, awsSettingsFileName)))
                   if (solution.FindProjectItem(awsSettingsFileName) == null)
                   {
                       await logger.InfoAsync($"Adding {awsSettingsFileName} to Solution Items folder");
                       solutionItemsFolder.Parent.ProjectItems.AddFromFile(
                           Path.Combine(solutionFolderPath, awsSettingsFileName));
                   }
                await logger.InfoAsync($"Generate {awsSettingsFileName} File complete");
           });
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
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
