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
using System.Windows.Shapes;

namespace PinboardGUI
{
    /// <summary>
    /// Interaction logic for ApiTokenPromptWindow.xaml
    /// </summary>
    public partial class ApiTokenPromptWindow : Window
    {
        private MainWindow ParentWindow;

        public ApiTokenPromptWindow(MainWindow ParentWindow)
        {
            InitializeComponent();
            this.ParentWindow = ParentWindow;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Setting apitoken to ==>  " + APIToken.Text); // TODO
            ParentWindow.ApiToken = APIToken.Text;
            DialogResult = true;
            Close();
        }
    }
}
