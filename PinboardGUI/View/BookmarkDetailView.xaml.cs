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

namespace PinboardGUI.View
{
    /// <summary>
    /// Interaction logic for BookmarkDetailView.xaml
    /// </summary>
    public partial class BookmarkDetailView : UserControl
    {
        public BookmarkDetailView()
        {
            InitializeComponent();
        }

        private void BookmarkDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ViewModel ==> " + ((MainWindow)Window.GetWindow(this.Parent)));
            // this.DataContext = ((MainWindow)Window.GetWindow(this)).ViewModel;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("context ===> " + this.DataContext);
        } 
    }
}
