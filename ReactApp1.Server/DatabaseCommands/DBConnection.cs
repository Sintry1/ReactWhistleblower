using MySql.Data.MySqlClient;
using DotNetEnv;

namespace ReactApp1
{
    public class DBConnection
    {

        private MySqlConnection connection;

        //Constructor for DBConnection
        private DBConnection()
        {
            // Initialize connection with an empty connection string
            connection = new MySqlConnection();
            SetDefaultConnection();
        }

        private void SetDefaultConnection()
        {
            // Set connection string with default values from .env file
            connection.ConnectionString = $"Server={Env.GetString("DB_SERVER")};Database={Env.GetString("DB_NAME")};";
        }

        // Set the connection credentials dynamically
        public void SetConnectionCredentials(string username, string password)
        {
            connection.ConnectionString += $"User ID={username};Password={password};";
        }

        //Method for opening connection to the database
        public MySqlConnection OpenConnection()
        {
            //tries to execute code
            try
            {
                //checks if the connection already is open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    //opens connection if connection isn't open
                    connection.Open();
                }
                //returns connection
                return connection;
            }
            //catches exeptions and returns null
            catch (Exception ex)
            {
                // Handle exceptions as needed
                //Console.WriteLine($"Error opening connection: {ex.Message}"); Need more secure way of handling error
                return null;
            }
        }

        //Method for closing connection
        public void CloseConnection()
        {
            try
            {
                //checks if the connection is already closed
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    //closes the connection if it isn't already closed
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                //Console.WriteLine($"Error closing connection: {ex.Message}"); Need more secure way of handling error
            }
        }

        //method for disposing connection, it also closes the connection before disposing of it
        public void Dispose()
        {
            CloseConnection();
            connection.Dispose();
        }
    }
}
