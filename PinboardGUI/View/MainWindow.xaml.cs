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
using PinboardGUI.ViewModel;

namespace PinboardGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PinboardViewModel ViewModel { get; private set; }
        public string ApiToken { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = new ApiTokenPromptWindow(this);
            bool? Result = w.ShowDialog();

            if ((bool)Result)
            {
                ViewModel = new PinboardViewModel(Dispatcher, ApiToken);
                this.DataContext = ViewModel;
            }
        }
    }
}
