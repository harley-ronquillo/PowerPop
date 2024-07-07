using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace PowerPop
{
    public partial class CalculateSubmeter : Page
    {
        private DbConn dbConn;

        public CalculateSubmeter()
        {
            InitializeComponent();
            dbConn = new DbConn(); // Initialize DbConn instance
        }

        private void calculate_btn(object sender, RoutedEventArgs e)
        {
            ClearResults();

            if (!ValidateEssentialInputs())
            {
                MessageBox.Show("Please enter valid values for Meralco Bill and kWh per Peso.");
                return;
            }

            decimal meralcoBill = decimal.Parse(meralco_bill.Text);
            decimal kWhPerPeso = decimal.Parse(kwh_peso.Text);

            try
            {
                CalculateAndDisplayBills(kWhPerPeso, meralcoBill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculating bills: {ex.Message}");
            }
        }

        private void CalculateAndDisplayBills(decimal kWhPerPeso, decimal meralcoBill)
        {
            string[] houseNumbers = { "A101", "B202", "C303", "D404", "E505" };
            decimal[] kWhUsages = new decimal[houseNumbers.Length];
            decimal[] bills = new decimal[houseNumbers.Length];
            decimal totalKWhUsage = 0;

            // Collect kWh usages and calculate total
            for (int i = 0; i < houseNumbers.Length; i++)
            {
                string usageText = ((TextBox)FindName($"kwh_hnumber{i + 1}"))?.Text;

                if (!string.IsNullOrWhiteSpace(usageText) && decimal.TryParse(usageText, out kWhUsages[i]))
                {
                    totalKWhUsage += kWhUsages[i];
                }
                else
                {
                    kWhUsages[i] = 0;
                }
            }

            // Calculate bills
            for (int i = 0; i < houseNumbers.Length; i++)
            {
                if (totalKWhUsage > 0)
                {
                    bills[i] = (kWhUsages[i] / totalKWhUsage) * meralcoBill;
                }
                else
                {
                    bills[i] = 0;
                }
            }

            // Adjust for rounding errors
            decimal totalCalculatedBill = 0;
            foreach (decimal bill in bills)
            {
                totalCalculatedBill += bill;
            }
            if (totalCalculatedBill != meralcoBill)
            {
                decimal difference = meralcoBill - totalCalculatedBill;
                for (int i = bills.Length - 1; i >= 0; i--)
                {
                    if (bills[i] > 0)
                    {
                        bills[i] += difference;
                        break;
                    }
                }
            }

            // Display results
            for (int i = 0; i < houseNumbers.Length; i++)
            {
                var billTextBlock = (TextBlock)FindName($"bill_hnumber{i + 1}");
                if (billTextBlock != null)
                {
                    billTextBlock.Text = bills[i].ToString("0.00");
                }
            }

            MessageBox.Show("Bills calculated successfully.");
        }

        private bool ValidateEssentialInputs()
        {
            if (string.IsNullOrWhiteSpace(meralco_bill.Text) ||
                string.IsNullOrWhiteSpace(kwh_peso.Text))
            {
                return false;
            }

            if (!decimal.TryParse(meralco_bill.Text, out _) ||
                !decimal.TryParse(kwh_peso.Text, out _))
            {
                return false;
            }

            return true;
        }

        private void ClearResults()
        {
            for (int i = 1; i <= 5; i++)
            {
                var billTextBlock = (TextBlock)FindName($"bill_hnumber{i}");
                if (billTextBlock != null)
                {
                    billTextBlock.Text = "";
                }
            }
        }

        private bool TryGetInputValues(out DateTime billingDate, out DateTime renterBillingDate, out decimal meralcoBill, out decimal kWhPerPeso)
        {
            billingDate = default;
            renterBillingDate = default;
            meralcoBill = default;
            kWhPerPeso = default;

            if (!DateTime.TryParse(billing_period.Text, out billingDate) || !DateTime.TryParse(renter_billing_period.Text, out renterBillingDate))
            {
                MessageBox.Show("Please enter valid dates for Billing Period and Due Date.");
                return false;
            }

            if (!decimal.TryParse(meralco_bill.Text, out meralcoBill) || meralcoBill < 0)
            {
                MessageBox.Show("Please enter a valid Meralco Bill amount.");
                return false;
            }

            if (!decimal.TryParse(kwh_peso.Text, out kWhPerPeso) || kWhPerPeso <= 0)
            {
                MessageBox.Show("Please enter a valid kWh per Peso value.");
                return false;
            }

            return true;
        }

        private void save_btn(object sender, RoutedEventArgs e)
        {
            if (!ValidateEssentialInputs())
            {
                MessageBox.Show("Please enter valid values for Meralco Bill and kWh per Peso.");
                return;
            }

            if (!TryGetInputValues(out DateTime billingDate, out DateTime renterBillingDate, out decimal meralcoBill, out decimal kWhPerPeso))
            {
                return;
            }

            try
            {
                int billingPeriodId = dbConn.SaveBillingPeriod(billingDate, renterBillingDate, meralcoBill, kWhPerPeso);

                for (int i = 1; i <= 5; i++)
                {
                    var houseNumberTextBlock = (TextBlock)FindName($"house_number{i}");
                    if (houseNumberTextBlock == null)
                    {
                        MessageBox.Show($"Element 'house_number{i}' not found.");
                        continue;
                    }

                    string houseNumber = houseNumberTextBlock.Text.Trim();
                    int houseId;
                    try
                    {
                        houseId = dbConn.GetHouseId(houseNumber);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error getting house ID for '{houseNumber}': {ex.Message}");
                        continue;
                    }

                    string kWhUsageText = ((TextBox)FindName($"kwh_hnumber{i}"))?.Text;
                    decimal kWhUsage = 0;
                    string status = "No Renter";
                    if (!string.IsNullOrWhiteSpace(kWhUsageText) && decimal.TryParse(kWhUsageText, out kWhUsage) && kWhUsage > 0)
                    {
                        status = "Pending";
                    }

                    string billText = ((TextBlock)FindName($"bill_hnumber{i}"))?.Text;
                    decimal billAmount = string.IsNullOrWhiteSpace(billText) ? 0 : decimal.Parse(billText);

                    // Log the parameters for debugging purposes
                    Console.WriteLine($"Executing query with parameters: BillingPeriodId={billingPeriodId}, HouseId={houseId}, kWhUsage={kWhUsage}, DueDate={renterBillingDate}, Status={status}");

                    dbConn.SaveRenterBill(billingPeriodId, houseId, kWhUsage > 0 ? kWhUsage : (decimal?)null, billAmount, renterBillingDate, status);
                }

                MessageBox.Show("Billing details saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving billing details: {ex.Message}");
            }
        }


    }
}
