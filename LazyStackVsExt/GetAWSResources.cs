using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using Microsoft.Win32;

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
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
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

            var solutionFullName = dte?.Solution?.FullName;
            if (string.IsNullOrEmpty(solutionFullName))
            {
                MessageBox.Show("Sorry - no solution is open.");
                return;
            }

            var solutionName = Path.GetFileName(solutionFullName);
            var solutionPath = Path.GetDirectoryName(solutionFullName);

            //// Check for ClientSDK project
            //var clientSDKPath = Path.Combine(Path.GetDirectoryName(solutionPath), $"{solutionName}ClientSDK");
            //if (!Directory.Exists(clientSDKPath))
            //{
            //    MessageBox.Show($"Sorry - your solution does not contain a {solutionName}ClientSDK project");
            //    return;
            //}


            var stackName = string.Empty;
            var region = string.Empty;
            bool canceled;
            do
            {
                var getStackNameDialog = new GetStackNameDialog();
                getStackNameDialog.StackName = stackName;
                getStackNameDialog.Region = region;
                getStackNameDialog.HasMinimizeButton = false;
                getStackNameDialog.HasMaximizeButton = false; 
                
                getStackNameDialog.ShowModal();

                stackName = getStackNameDialog.StackName;
                region = getStackNameDialog.Region;
                canceled = getStackNameDialog.Canceled;

                if (!string.IsNullOrEmpty(stackName))
                {
                    try
                    {
                        var awsSettings = new AwsSettings(stackName, region);
                        var json = awsSettings.BuildJson();

                        var saveFileDialog = new SaveFileDialog() 
                        { 
                            FileName = $"{stackName}Settings.json",
                            Title = "Save AWS Settings File",
                            Filter = "JSON Settings|*.json"
                        
                        };
                        saveFileDialog.ShowDialog();

                        if (!string.IsNullOrEmpty(saveFileDialog.FileName))
                        {
                            File.WriteAllText(saveFileDialog.FileName, json);

                            var solution = dte?.Solution as Solution4;

                            // create SolutionItems folder if it does not exist
                            var solutionItemsFolder = CreateSolutionFolder("SolutionItems");

                            // Add settings file to SolutionItems folder if it does not already exist
                            if (File.Exists(saveFileDialog.FileName))
                                if (solution.FindProjectItem(Path.GetFileName(saveFileDialog.FileName)) == null)
                                {
                                    solutionItemsFolder.Parent.ProjectItems.AddFromFile(saveFileDialog.FileName);
                                }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            while (!canceled);
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
