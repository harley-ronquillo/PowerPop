using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace PowerPop
{
    public class DbConn
    {
        private string connString = "Server=localhost;Port=3306;Database=powerpopdb;Uid=root;Pwd=;";
        private MySqlConnection mySqlConnection;

        public DbConn()
        {
            mySqlConnection = new MySqlConnection(connString);
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

                mySqlConnection.Open();
                int billingPeriodId = Convert.ToInt32(cmd.ExecuteScalar());
                mySqlConnection.Close();

                return billingPeriodId;
            }
        }

        public void SaveRenterBill(int billingPeriodId, int houseId, decimal kWhUsage, decimal billAmount, DateTime dueDate)
        {
            string query = @"INSERT INTO renter_bills (billing_period_id, house_id, kwh_usage, bill_amount, due_date, status) 
                     VALUES (@BillingPeriodId, @HouseId, @kWhUsage, @BillAmount, @DueDate, 'Pending')";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@BillingPeriodId", billingPeriodId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.Parameters.AddWithValue("@kWhUsage", kWhUsage);
                cmd.Parameters.AddWithValue("@BillAmount", billAmount);
                cmd.Parameters.AddWithValue("@DueDate", dueDate);

                mySqlConnection.Open();
                cmd.ExecuteNonQuery();
                mySqlConnection.Close();
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


        public int GetHouseId(string houseNumber)
        {
            string query = "SELECT house_id FROM houses WHERE house_number = @HouseNumber";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@HouseNumber", houseNumber);
                mySqlConnection.Open();
                object result = cmd.ExecuteScalar();
                mySqlConnection.Close();

                if (result == null || result == DBNull.Value)
                    throw new Exception("House not found for house number: " + houseNumber);

                return Convert.ToInt32(result);
            }
        }

        public void SaveBillingDetails(int houseId, DateTime billingDate, decimal amount, DateTime dueDate)
        {
            string query = @"INSERT INTO billing_details (house_id, billing_date, amount, due_date) 
                             VALUES (@HouseId, @BillingDate, @Amount, @DueDate)";
            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.Parameters.AddWithValue("@BillingDate", billingDate);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@DueDate", dueDate);

                mySqlConnection.Open();
                cmd.ExecuteNonQuery();
                mySqlConnection.Close();
            }
        }

        private void ExecuteNonQuery(MySqlCommand cmd)
        {
            try
            {
                mySqlConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing SQL: " + ex.Message);
            }
            finally
            {
                mySqlConnection.Close();
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
