using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace PowerPop
{
    public class DbConn
    {
        private string connString;
        private MySqlConnection mySqlConnection;

        public DbConn()
        {
            connString = "Server=localhost;Port=3306;Database=powerpopdb;Uid=root;Pwd=root;";
            mySqlConnection = new MySqlConnection(connString);
        }

        public MySqlConnection GetConnection()
        {
            return mySqlConnection;
        }

        public void OpenConnection()
        {
            if (mySqlConnection.State == ConnectionState.Closed)
            {
                mySqlConnection.Open();
            }
        }

        public void CloseConnection()
        {
            if (mySqlConnection.State == ConnectionState.Open)
            {
                mySqlConnection.Close();
            }
        }

        public int SaveBillingPeriod(DateTime billingDate, DateTime renterBillingDate, decimal meralcoBill, decimal kWhPerPeso)
        {
            string query = @"INSERT INTO billing_periods (billing_date, renter_billing_date, meralco_bill, kwh_per_peso) 
                             VALUES (@BillingDate, @RenterBillingDate, @MeralcoBill, @kWhPerPeso);
                             SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@BillingDate", billingDate);
                cmd.Parameters.AddWithValue("@RenterBillingDate", renterBillingDate);
                cmd.Parameters.AddWithValue("@MeralcoBill", meralcoBill);
                cmd.Parameters.AddWithValue("@kWhPerPeso", kWhPerPeso);

                OpenConnection(); // Open connection before executing command
                int billingPeriodId = Convert.ToInt32(cmd.ExecuteScalar());
                CloseConnection(); // Close connection after executing command

                return billingPeriodId;
            }
        }

        public void SaveRenterBill(int billingPeriodId, int houseId, decimal? kWhUsage, decimal billAmount, DateTime dueDate, string status)
        {
            // Default status to "No Renter" if null or empty
            status = string.IsNullOrWhiteSpace(status) ? "No Renter" : status;

            string query = @"INSERT INTO renter_bills (billing_period_id, house_id, kwh_usage, bill_amount, due_date, status) 
                     VALUES (@BillingPeriodId, @HouseId, @kWhUsage, @BillAmount, @DueDate, @Status)";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@BillingPeriodId", billingPeriodId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.Parameters.AddWithValue("@kWhUsage", kWhUsage.HasValue ? (object)kWhUsage.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@BillAmount", billAmount);
                cmd.Parameters.AddWithValue("@DueDate", dueDate);
                cmd.Parameters.AddWithValue("@Status", status);

                

                try
                {
                    OpenConnection(); // Open connection before executing command
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving renter bill: {ex.Message}");
                }
                finally
                {
                    CloseConnection(); // Close connection after executing command
                }
            }
        }





        public int GetHouseId(string houseNumber)
        {
            string query = "SELECT house_id FROM houses WHERE house_number = @HouseNumber";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@HouseNumber", houseNumber);
                OpenConnection(); // Open connection before executing command
                object result = cmd.ExecuteScalar();
                CloseConnection(); // Close connection after executing command

                if (result == null || result == DBNull.Value)
                    throw new Exception("House not found for house number: " + houseNumber);

                return Convert.ToInt32(result);
            }
        }

        public DataTable GetData(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private void ExecuteNonQuery(MySqlCommand cmd)
        {
            try
            {
                OpenConnection(); // Open connection before executing command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing SQL: " + ex.Message);
            }
            finally
            {
                CloseConnection(); // Close connection after executing command
            }
        }

        public void ExecuteCommand(string query)
        {
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                ExecuteNonQuery(cmd);
            }
        }
    }
}
