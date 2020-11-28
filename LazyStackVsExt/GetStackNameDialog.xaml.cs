using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;

namespace LazyStackVsExt
{
    /// <summary>
    /// Interaction logic for GetStackNameDialog.xaml
    /// </summary>
    public partial class GetStackNameDialog : DialogWindow
    {
        public GetStackNameDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string StackName { get; set; }
        public string Region { get; set; }
        public bool Canceled;

        private void getResourcesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StackName) || string.IsNullOrEmpty(Region))
                MessageBox.Show("Please enter a AWS stack name and region");
            else
                Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog box
            StackName = string.Empty;
            Region = string.Empty;
            Canceled = true;
            Close();
        }

	}
}
