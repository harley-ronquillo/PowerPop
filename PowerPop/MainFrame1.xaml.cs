using System.Windows;
using System.Windows.Controls;

namespace PowerPop
{
    public partial class MainFrame1 : Window
    {
        public MainFrame1()
        {
            InitializeComponent(); // Error: 'InitializeComponent' does not exist in the current context
        }

        private void calculatewindowbtn(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CalculateSubmeter()); // Error: 'MainFrame' does not exist in the current context
        }

        private void billingrecordbtn(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BillingRecords()); // Error: 'MainFrame' does not exist in the current context
        }
    }
}
