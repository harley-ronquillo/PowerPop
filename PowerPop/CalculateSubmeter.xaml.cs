using System;
using System.Windows;
using System.Windows.Controls;

namespace PowerPop
{
    public partial class CalculateSubmeter : Page
    {
        public CalculateSubmeter()
        {
            InitializeComponent();
        }

        private void calculate_btn(object sender, RoutedEventArgs e)
        {
            ClearResults();

            if (!ValidateInputs())
            {
                MessageBox.Show("Please enter valid values for all fields.");
                return;
            }

            DateTime billingDate;
            DateTime renterBillingDate;
            decimal meralcoBill;
            decimal kWhPerPeso;

            if (!DateTime.TryParse(billing_period.Text, out billingDate) || !DateTime.TryParse(renter_billing_period.Text, out renterBillingDate))
            {
                MessageBox.Show("Please enter valid dates for Billing Period and Due Date.");
                return;
            }

            if (!decimal.TryParse(meralco_bill.Text, out meralcoBill) || meralcoBill < 0)
            {
                MessageBox.Show("Please enter a valid Meralco Bill amount.");
                return;
            }

            if (!decimal.TryParse(kwh_peso.Text, out kWhPerPeso) || kWhPerPeso <= 0)
            {
                MessageBox.Show("Please enter a valid kWh per Peso value.");
                return;
            }

            CalculateAndDisplayBills(kWhPerPeso, meralcoBill);
        }

        private void CalculateAndDisplayBills(decimal kWhPerPeso, decimal meralcoBill)
        {
            string[] houseNumbers = { "A101", "B202", "C303", "D404", "E505" };
            decimal[] kWhUsages = new decimal[5];
            decimal[] initialBills = new decimal[5];
            decimal totalCalculatedBill = 0;

            for (int i = 0; i < houseNumbers.Length; i++)
            {
                kWhUsages[i] = decimal.Parse(((TextBox)FindName($"kwh_hnumber{i + 1}")).Text);
                initialBills[i] = kWhUsages[i] * kWhPerPeso;
                totalCalculatedBill += initialBills[i];
            }

            if (totalCalculatedBill != meralcoBill)
            {
                decimal difference = meralcoBill - totalCalculatedBill;
                decimal totalUsage = 0;
                for (int i = 0; i < kWhUsages.Length; i++)
                {
                    totalUsage += kWhUsages[i];
                }

                for (int i = 0; i < initialBills.Length; i++)
                {
                    initialBills[i] += (kWhUsages[i] / totalUsage) * difference;
                }

                // Adjust for rounding errors
                decimal finalCalculatedBill = 0;
                for (int i = 0; i < initialBills.Length; i++)
                {
                    finalCalculatedBill += initialBills[i];
                }

                if (finalCalculatedBill != meralcoBill)
                {
                    decimal roundingError = meralcoBill - finalCalculatedBill;
                    initialBills[initialBills.Length - 1] += roundingError;
                }
            }

            for (int i = 0; i < houseNumbers.Length; i++)
            {
                ((TextBlock)FindName($"bill_hnumber{i + 1}")).Text = initialBills[i].ToString("0.00");
            }

            MessageBox.Show("Bills calculated successfully.");
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(billing_period.Text) || string.IsNullOrWhiteSpace(renter_billing_period.Text) ||
                string.IsNullOrWhiteSpace(meralco_bill.Text) || string.IsNullOrWhiteSpace(kwh_peso.Text) ||
                string.IsNullOrWhiteSpace(kwh_hnumber1.Text) || string.IsNullOrWhiteSpace(kwh_hnumber2.Text) ||
                string.IsNullOrWhiteSpace(kwh_hnumber3.Text) || string.IsNullOrWhiteSpace(kwh_hnumber4.Text) ||
                string.IsNullOrWhiteSpace(kwh_hnumber5.Text))
            {
                return false;
            }

            return true;
        }

        private void ClearResults()
        {
            for (int i = 1; i <= 5; i++)
            {
                ((TextBlock)FindName($"bill_hnumber{i}")).Text = "";
            }
        }

        private void save_btn(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Please enter valid values for all fields.");
                return;
            }

            DateTime billingDate;
            DateTime renterBillingDate;
            decimal meralcoBill;
            decimal kWhPerPeso;

            if (!DateTime.TryParse(billing_period.Text, out billingDate) || !DateTime.TryParse(renter_billing_period.Text, out renterBillingDate))
            {
                MessageBox.Show("Please enter valid dates for Billing Period and Due Date.");
                return;
            }

            if (!decimal.TryParse(meralco_bill.Text, out meralcoBill) || meralcoBill < 0)
            {
                MessageBox.Show("Please enter a valid Meralco Bill amount.");
                return;
            }

            if (!decimal.TryParse(kwh_peso.Text, out kWhPerPeso) || kWhPerPeso <= 0)
            {
                MessageBox.Show("Please enter a valid kWh per Peso value.");
                return;
            }

            DbConn dbConn = new DbConn();
            int billingPeriodId = dbConn.SaveBillingPeriod(billingDate, renterBillingDate, meralcoBill, kWhPerPeso);

            for (int i = 1; i <= 5; i++)
            {
                string houseNumberControlName = $"house_number{i}";
                TextBlock houseNumberTextBlock = (TextBlock)FindName(houseNumberControlName);

                if (houseNumberTextBlock != null)
                {
                    string houseNumber = houseNumberTextBlock.Text.Trim();
                    int houseId = dbConn.GetHouseId(houseNumber);

                    if (houseId != -1)
                    {
                        decimal kWhUsage = decimal.Parse(((TextBox)FindName($"kwh_hnumber{i}")).Text);
                        decimal billAmount = decimal.Parse(((TextBlock)FindName($"bill_hnumber{i}")).Text);
                        dbConn.SaveRenterBill(billingPeriodId, houseId, kWhUsage, billAmount, renterBillingDate);
                    }
                }
            }

            MessageBox.Show("Billing details saved successfully.");
        }
    }
}
