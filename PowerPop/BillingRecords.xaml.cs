using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            public string DueDate { get; set; }
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
            graphics.DrawString("Due Date: " + selectedRecord.DueDate, bodyFont, Brushes.Black, x, y);
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
                        DueDate = row["due_date"].ToString(),
                        KWhPerPeso = Convert.ToDecimal(row["kwh_per_peso"]),
                        BillPerHouseNo = Convert.ToDecimal(row["bill_per_house_no"]),
                        Status = row["status"].ToString()
                    });
                }

                dataGrid.ItemsSource = billingRecords;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading billing records: {ex.Message}");
            }
        }
    }
}
