using System;
using System.Collections.Generic;
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

namespace PluginLoaderInstaller.Views
{
    /// <summary>
    /// Interaction logic for InstallationPageView.xaml
    /// </summary>
    public partial class InstallationPageView : UserControl
    {
        public InstallationPageView()
        {
            InitializeComponent();
        }

        private void TextBoxLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }
    }
}
