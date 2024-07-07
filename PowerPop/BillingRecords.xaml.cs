using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization; // For DateTime parsing
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // For ICollectionView
using System.Windows.Forms; // Add this if you're using MessageBox from System.Windows.Forms

namespace PowerPop
{
    public partial class BillingRecords : Page
    {
        private DbConn dbConn;
        private BillingRecord selectedRecord;
        private PrintDocument printDocument;

        public BillingRecords()
        {
            InitializeComponent();
            this.DataContext = this;
            dbConn = new DbConn();
            LoadBillingRecords();
        }

        public class BillingRecord
        {
            public int HouseId { get; set; }
            public int BillingPeriodId { get; set; }
            public string HouseNumber { get; set; }
            public string BillingPeriod { get; set; }
            public decimal MeralcoBill { get; set; }
            public DateTime DueDate { get; set; } // Changed to DateTime
            public decimal KWhPerPeso { get; set; }
            public decimal BillPerHouseNo { get; set; }
            public string Status { get; set; }
        }

        private void savebtn_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null && statusComboBox.SelectedItem != null)
            {
                selectedRecord = (BillingRecord)dataGrid.SelectedItem;
                selectedRecord.Status = ((ComboBoxItem)statusComboBox.SelectedItem).Content.ToString();

                try
                {
                    dbConn.ExecuteCommand($"UPDATE renter_bills SET status = '{selectedRecord.Status}' WHERE house_id = {selectedRecord.HouseId} AND billing_period_id = {selectedRecord.BillingPeriodId}");
                    System.Windows.MessageBox.Show("Status updated successfully.");
                    LoadBillingRecords();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error updating status: {ex.Message}");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a record and a status.");
            }
        }

        private void printbtn_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                selectedRecord = (BillingRecord)dataGrid.SelectedItem;

                // Initialize the PrintDocument
                printDocument = new PrintDocument();
                printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

                // Create a PrintPreviewDialog
                PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
                printPreviewDialog.Document = printDocument;

                // Show print preview dialog
                if (printPreviewDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Optionally handle post-preview actions here
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a record to print.");
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            System.Drawing.Font titleFont = new System.Drawing.Font(new FontFamily("Arial"), 20, System.Drawing.FontStyle.Bold);
            System.Drawing.Font bodyFont = new System.Drawing.Font(new FontFamily("Arial"), 12);
            float lineHeight = bodyFont.GetHeight();
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;

            // Calculate total width of the printable area
            float totalWidth = e.MarginBounds.Width;

            // Centering the title horizontally
            string title = "===== PowerPop Sub Meter Bill =====";
            float titleWidth = graphics.MeasureString(title, titleFont).Width;
            float titleX = x + (totalWidth - titleWidth) / 2;
            graphics.DrawString(title, titleFont, Brushes.Black, titleX, y);
            y += lineHeight + 20;

            // Centering the content horizontally
            string billingTitle = "===== Billing Record =====";
            float billingTitleWidth = graphics.MeasureString(billingTitle, titleFont).Width;
            float billingTitleX = x + (totalWidth - billingTitleWidth) / 2;
            graphics.DrawString(billingTitle, titleFont, Brushes.Black, billingTitleX, y);
            y += lineHeight + 20;

            // Now print the details vertically centered
            graphics.DrawString("House Number: " + selectedRecord.HouseNumber, bodyFont, Brushes.Black, x, y);
            y += lineHeight;
            graphics.DrawString("Billing Period: " + selectedRecord.BillingPeriod, bodyFont, Brushes.Black, x, y);
            y += lineHeight;
            graphics.DrawString("Due Date: " + selectedRecord.DueDate.ToString("yyyy-MM-dd"), bodyFont, Brushes.Black, x, y); // Format DueDate
            y += lineHeight;
            graphics.DrawString("Meralco Bill: ₱" + selectedRecord.MeralcoBill.ToString("#,##0.00"), bodyFont, Brushes.Black, x, y);
            y += lineHeight;
            graphics.DrawString("Bill Per House No.: ₱" + selectedRecord.BillPerHouseNo.ToString("#,##0.00"), bodyFont, Brushes.Black, x, y);
            y += lineHeight;
        }

        private void LoadBillingRecords()
        {
            try
            {
                string query = @"SELECT 
                            rb.renter_bill_id, 
                            rb.billing_period_id, 
                            rb.house_id, 
                            h.house_number, 
                            bp.billing_date AS billing_period, 
                            bp.meralco_bill, 
                            bp.kwh_per_peso, 
                            rb.bill_amount AS bill_per_house_no, 
                            rb.due_date, 
                            rb.status
                         FROM 
                            renter_bills rb
                            INNER JOIN houses h ON rb.house_id = h.house_id
                            INNER JOIN billing_periods bp ON rb.billing_period_id = bp.billing_period_id";

                DataTable dataTable = dbConn.GetData(query);
                List<BillingRecord> billingRecords = new List<BillingRecord>();

                foreach (DataRow row in dataTable.Rows)
                {
                    billingRecords.Add(new BillingRecord
                    {
                        HouseId = Convert.ToInt32(row["house_id"]),
                        BillingPeriodId = Convert.ToInt32(row["billing_period_id"]),
                        HouseNumber = row["house_number"].ToString(),
                        BillingPeriod = row["billing_period"].ToString(),
                        MeralcoBill = Convert.ToDecimal(row["meralco_bill"]),
                        DueDate = DateTime.Parse(row["due_date"].ToString(), CultureInfo.InvariantCulture), // Parse DateTime
                        KWhPerPeso = Convert.ToDecimal(row["kwh_per_peso"]),
                        BillPerHouseNo = Convert.ToDecimal(row["bill_per_house_no"]),
                        Status = row["status"].ToString()
                    });
                }

                // Bind the sorted view to DataGrid
                dataGrid.ItemsSource = billingRecords;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading billing records: {ex.Message}");
            }
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Implement custom sorting for DueDate column
            if (e.Column.Header.ToString() == "Due Date")
            {
                e.Handled = true;
                ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ?
                    ListSortDirection.Ascending : ListSortDirection.Descending;
                e.Column.SortDirection = direction;

                // Cast ItemsSource to ICollectionView to use SortDescriptions
                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("DueDate", direction));
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Set custom sorting event handler for DataGrid
            if (e.Column.Header.ToString() == "DueDate")
            {
                DataGridColumn column = e.Column;
                column.SortMemberPath = "DueDate";
                column.CanUserSort = true;
                column.SortDirection = null;
            }
        }
    }
}
