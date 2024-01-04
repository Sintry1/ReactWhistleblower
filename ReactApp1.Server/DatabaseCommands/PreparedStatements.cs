using DotNetEnv;
using MySql.Data.MySqlClient;

namespace ReactApp1
{
    public class PreparedStatements
    {
        /* Makes a private dbConnection of type DBConnection, that can only be written to in initialization or in the constructor
         * this ensures that the connection can only be READ by other classes and not modified
         */
        private readonly DBConnection dbConnection;

        //Constructor for PreparedStatements
        private PreparedStatements()
        {
            dbConnection = DBConnection.CreateInstance();
        }

        //Method for creating an instance of PreparedStatements
        //This is needed as the constructor above is private for security.
        public static PreparedStatements CreateInstance()
        {
            return new PreparedStatements();
        }

        //This method checks if the user exists in our database, this method may be COMPLETELY obsolete, thanks to firebase
        public bool ExistingUser(string userName)
        {
            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(Env.GetString("OTHERS_READER_NAME"), Env.GetString("OTHERS_READER_PASSWORD"));

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT CASE WHEN EXISTS (SELECT 1 FROM regulators WHERE regulator_name = @userName) THEN CAST('TRUE' AS BIT) ELSE CAST('FALSE' AS BIT) END";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("userName", userName);

                    // Adds the parameter to the command
                    command.Parameters.Add(userNameParam);

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query and cast the result to a boolean
                    bool userExists = (bool)command.ExecuteScalar();

                    //returns true after everything is done.
                    return userExists;
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        //Stores username and the hashedPassword, this method may be COMPLETELY obsolete, thanks to firebase
        public void StoreRegulatorInformation(string userName, string hash, byte[] publicKey, byte[] encryptedPrivateKey, string industryName)
        {
            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(Env.GetString("REGULATOR_WRITER_NAME"), Env.GetString("REGULATOR_WRITER_PASSWORD"));

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {

                    // Query to get industry_id based on industryName
                    string industryIdQuery = "SELECT industry_id FROM industry WHERE industry_name = @industry_name";

                    // Create and prepare an SQL statement for industry_id
                    MySqlCommand industryIdCommand = new MySqlCommand(industryIdQuery, connection);
                    industryIdCommand.Parameters.AddWithValue("@industry_name", industryName);
                    industryIdCommand.Prepare();

                    // Execute the query to get industry_id
                    int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());

                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"INSERT INTO regulators (regulator_name, password, public_key, private_key, industry_id) VALUES (@userName, @hash, @publicKey, @privateKey, industry_id)";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("userName", userName);
                    MySqlParameter hashParam = new MySqlParameter("hash", hash);
                    MySqlParameter publicKeyParam = new MySqlParameter("publicKey", publicKey);
                    MySqlParameter privateKeyParam = new MySqlParameter("privateKey", encryptedPrivateKey);
                    MySqlParameter industryIDParam = new MySqlParameter("industry_id", industryId);

                    // Adds the parameter to the command
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(hashParam);
                    command.Parameters.Add(publicKeyParam);
                    command.Parameters.Add(privateKeyParam);
                    command.Parameters.Add(industryIDParam);

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query and cast the result to a boolean
                    command.ExecuteNonQuery();
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        public byte[] GetPrivateKey(string industryName)
        {
            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(Env.GetString("REGULATOR_WRITER_NAME"), Env.GetString("REGULATOR_WRITER_PASSWORD"));

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    // Query to get industry_id based on industryName
                    string industryIdQuery = "SELECT industry_id FROM industry WHERE industry_name = @industry_name";

                    // Create and prepare an SQL statement for industry_id
                    MySqlCommand industryIdCommand = new MySqlCommand(industryIdQuery, connection);
                    industryIdCommand.Parameters.AddWithValue("industry_name", industryName);
                    industryIdCommand.Prepare();

                    // Execute the query to get industry_id
                    int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());

                    // Query to get private_key based on industry_id
                    string privateKeyQuery = "SELECT private_key FROM regulators WHERE industry_id = @industry_id";

                    // Create and prepare an SQL statement for private_key
                    MySqlCommand privateKeyCommand = new MySqlCommand(privateKeyQuery, connection);
                    privateKeyCommand.Parameters.AddWithValue("industry_id", industryId);
                    privateKeyCommand.Prepare();

                    // Execute the query to get private_key
                    byte[] privateKey = (byte[])privateKeyCommand.ExecuteScalar();

                    return privateKey;
                }catch (MySqlException ex)
                {
                    return null;
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        //Gets the stores has of the user's password, this method may be COMPLETELY obsolete, thanks to firebase
        public string GetHashedPassword(string userName)
        {

            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(Env.GetString("OTHERS_READER_NAME"), Env.GetString("OTHERS_READER_PASSWORD"));

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {

                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT password FROM regulators WHERE regulator_name = @regulator_name";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("regulator_name", userName);

                    // Adds the parameter to the command
                    command.Parameters.Add(userNameParam);

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query
                    object result = command.ExecuteScalar();

                    //Casts the result to string
                    string storedHash = result.ToString();

                    //returns the hashed password
                    return storedHash;
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        /*
         * Takes an object of type Report, made using the Report class
         * Tries to takes parameters from the object and sets them as paramaters for the prepared statement
         * Returns true to the function that called it IF it succeds
         * it returns false if it fails/catches an error
         */
        public bool StoreReport(Report report)
        {
            try
            {
                // Set credentials for the user needed
                dbConnection.SetConnectionCredentials(Env.GetString("REPORTS_WRITER_NAME"), Env.GetString("REPORTS_WRITER_PASSWORD"));

                // Use mySqlConnection to open the connection and throw an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    Console.WriteLine("Connection opened successfully.");

                    try
                    {
                        // Query to get industry_id based on industryName
                        string industryIdQuery = "SELECT industry_id FROM industry WHERE industry_name = @industry_name";
                        Console.WriteLine($"Executing query: {industryIdQuery}");

                        // Create and prepare an SQL statement for industry_id
                        MySqlCommand industryIdCommand = new MySqlCommand(industryIdQuery, connection);
                        industryIdCommand.Parameters.AddWithValue("industry_name", report.IndustryName);
                        industryIdCommand.Prepare();

                        // Execute the query to get industry_id
                        int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());
                        Console.WriteLine($"Got industry_id: {industryId}");

                        // Create an instance of MySqlCommand
                        MySqlCommand command = new MySqlCommand(null, connection);

                        // Create and prepare an SQL statement.
                        command.CommandText =
                            $"INSERT INTO reports (industry_id, company_name, description, email) VALUES (@industry_id, @company_name, @description, @email)";
                        Console.WriteLine($"Executing query: {command.CommandText}");

                        // Sets mySQL parameters for the prepared statement
                        MySqlParameter industryIDParam = new MySqlParameter("industry_id", industryId);
                        MySqlParameter companyNameParam = new MySqlParameter("company_name", report.CompanyName);
                        MySqlParameter msgParam = new MySqlParameter("description", report.Description);

                        // Check if email is null, and set the parameter accordingly
                        MySqlParameter emailParam;
                        if (string.IsNullOrEmpty(report.Email))
                        {
                            emailParam = new MySqlParameter("email", DBNull.Value);
                        }
                        else
                        {
                            emailParam = new MySqlParameter("email", report.Email);
                        }

                        // Adds the parameters to the command
                        command.Parameters.Add(industryIDParam);
                        command.Parameters.Add(companyNameParam);
                        command.Parameters.Add(msgParam);
                        command.Parameters.Add(emailParam);

                        // Call Prepare after setting the Commandtext and Parameters.
                        command.Prepare();

                        // Execute the query
                        object result = command.ExecuteScalar();
                        Console.WriteLine($"Query executed successfully. Result: {result}");

                        // Return true if no exceptions are thrown
                        return true;
                    }
                    catch (MySqlException ex)
                    {
                        // Handle the exception (e.g., log it) and return false
                        // You may want to implement secure logging to store the error message
                        Console.WriteLine($"Error executing query: {ex.Message}");
                        return false;
                    }
                    finally
                    {
                        // Close the connection at the end
                        dbConnection.CloseConnection();
                        Console.WriteLine("Connection closed.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception if opening the connection fails
                Console.WriteLine($"Error opening connection: {ex.Message}");
                return false;
            }
        }


        /*
         * Fetches all reports that have the same industry ID as passed to the function
         * All information is turned into a "Report" object
         * the function then returns a list of all Reports
         */
        public List<Report> GetAllReportsByIndustryName(string industryName)
        {
            List<Report> reports = new List<Report>();

            // Set credentials for the user needed
            dbConnection.SetConnectionCredentials(Env.GetString("REPORTS_READER_NAME"), Env.GetString("REPORTS_READER_PASSWORD"));

            // Use mySqlConnection to open the connection and throw an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    // Query to get industry_id based on industryName
                    string industryIdQuery = "SELECT industry_id FROM industry WHERE industry_name = @industry_name";

                    // Create and prepare an SQL statement for industry_id
                    MySqlCommand industryIdCommand = new MySqlCommand(industryIdQuery, connection);
                    industryIdCommand.Parameters.AddWithValue("industry_name", industryName);
                    industryIdCommand.Prepare();

                    // Execute the query to get industry_id
                    int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());

                    // Create an instance of MySqlCommand
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        "SELECT * FROM reports WHERE industry_id = @industry_id";

                    // Set mySQL parameters for the prepared statement
                    MySqlParameter industryIDParam = new MySqlParameter("industry_id", industryId);
                    command.Parameters.Add(industryIDParam);

                    // Execute the query
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Read data from the database and create a Report object
                            int reportID = reader.GetInt32("report_id");
                            string companyName = reader.GetString("company_name");
                            string description = reader.GetString("description");
                            string email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email");

                            Report report = new Report(industryName, companyName, description, email);
                            reports.Add(report);
                        }
                    }

                    // Return the list of reports
                    return reports;
                }
                catch (MySqlException ex)
                {
                    // Handle the exception (e.g., log it) and return an empty list or null
                    // You may want to implement secure logging to store the error message
                    return new List<Report>();
                }
                finally
                {
                    // Close the connection at the end
                    dbConnection.CloseConnection();
                }
            }
        }


        public byte[] GetPublicKey(string industryName)
        {
            try
            {
                // Set credentials for the user needed
                dbConnection.SetConnectionCredentials(Env.GetString("OTHERS_READER_NAME"), Env.GetString("OTHERS_READER_PASSWORD"));

                // Use MySqlConnection to open the connection and throw an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    Console.WriteLine("Connection opened successfully.");

                    try
                    {
                        // Query to get industry_id based on industryName
                        string industryIdQuery = "SELECT industry_id FROM industry WHERE industry_name = @industry_name";
                        Console.WriteLine($"Executing query: {industryIdQuery}");

                        // Create and prepare an SQL statement for industry_id
                        MySqlCommand industryIdCommand = new MySqlCommand(industryIdQuery, connection);
                        industryIdCommand.Parameters.AddWithValue("industry_name", industryName);
                        industryIdCommand.Prepare();

                        // Execute the query to get industry_id
                        int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());
                        Console.WriteLine($"Got industry_id: {industryId}");

                        // Create an instance of MySqlCommand
                        MySqlCommand command = new MySqlCommand(null, connection);

                        // Create and prepare an SQL statement.
                        command.CommandText =
                            $"SELECT public_key FROM regulators WHERE industry_id = @industry_id";
                        Console.WriteLine($"Executing query: {command.CommandText}");

                        // Sets mySQL parameters for the prepared statement
                        MySqlParameter industryIDParam = new MySqlParameter("industry_id", industryId);

                        // Adds the parameters to the command
                        command.Parameters.Add(industryIDParam);

                        // Call Prepare after setting the Commandtext and Parameters.
                        command.Prepare();

                        // Execute the query and cast the result to a byte array
                        object result = command.ExecuteScalar();
                        Console.WriteLine("Query executed successfully.");

                        // Return the byte array
                        return result as byte[];
                    }
                    catch (MySqlException ex)
                    {
                        // Return null if an exception is thrown, may want to implement secure logging
                        Console.WriteLine($"Error executing query: {ex.Message}");
                        return null;
                    }
                    finally
                    {
                        // Close the connection at the end
                        dbConnection.CloseConnection();
                        Console.WriteLine("Connection closed.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception if opening the connection fails
                Console.WriteLine($"Error opening connection: {ex.Message}");
                return null;
            }
        }

    }
}