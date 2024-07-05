using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PowerPop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void calculatewindowbtn(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CalculateSubmeter());
        }

        private void billingrecordbtn(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BillingRecords());
        }
    }
}