using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Reflection.PortableExecutable;

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
            Env.Load();
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
            Console.WriteLine("Setting connection credentials...");
            dbConnection.SetConnectionCredentials(Env.GetString("OTHER_READER_NAME"), Env.GetString("OTHER_READER_PASSWORD"));

            Console.WriteLine("Opening connection...");
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    Console.WriteLine("Creating command...");
                    MySqlCommand command = new MySqlCommand(null, connection);

                    Console.WriteLine("Preparing SQL statement for checking user exists");
                    command.CommandText =
                        $"SELECT CASE WHEN EXISTS (SELECT 1 FROM regulators WHERE regulator_name = @userName) THEN 1 ELSE 0 END";

                    Console.WriteLine("Setting parameter for user exists");
                    Console.WriteLine($"username being set to {userName}");
                    MySqlParameter userNameParam = new MySqlParameter("@userName", userName);

                    Console.WriteLine("Adding parameter to command  for user exists");
                    command.Parameters.Add(userNameParam);

                    Console.WriteLine("Preparing command for user exists");
                    command.Prepare();

                    Console.WriteLine($"Executing query for user exists with parameter value: {userName}");

                    // Debug: Print out the actual SQL query with parameter values
                    Console.WriteLine($"SQL Query: {command.CommandText}");
                    Console.WriteLine($"Parameter Values: {userNameParam.Value}");

                    // Execute the query and cast the result to a long
                    long result = (long)command.ExecuteScalar();

                    Console.WriteLine($"int64 value: {result}");

                    // Convert the long result to boolean
                    bool userExists = result == 1;

                    Console.WriteLine("Query executed. User exists: " + userExists);
                    return userExists;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    return false;

                }
                finally
                {
                    Console.WriteLine("Closing connection...");
                    dbConnection.CloseConnection();
                    Console.WriteLine("Connection closed.");
                }
            }
        }

        //Stores username and the hashedPassword, this method may be COMPLETELY obsolete, thanks to firebase
        public void StoreRegulatorInformation(
            string userName,
            string hash,
            string publicKey,
            string encryptedPrivateKey,
            string industryName,
            string iv,
            string salt
        )
        {
            //Calls another prepared statement to get the industry ID from the industry name
            int industryId = GetIndustryID(industryName);

            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(
                Env.GetString("REGULATOR_WRITER_NAME"),
                Env.GetString("REGULATOR_WRITER_PASSWORD")
            );
            Console.WriteLine($"encrypted key stored: {encryptedPrivateKey}");
            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"INSERT INTO regulators (regulator_name, password,iv ,public_key, private_key, industry_id, salt) VALUES (@userName, @hash,  @iv, @publicKey, @privateKey, @industry_id, @salt)";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("userName", userName);
                    MySqlParameter hashParam = new MySqlParameter("hash", hash);
                    MySqlParameter ivParam = new MySqlParameter("iv", iv);
                    MySqlParameter publicKeyParam = new MySqlParameter("publicKey", publicKey);
                    MySqlParameter privateKeyParam = new MySqlParameter("privateKey", encryptedPrivateKey);
                    MySqlParameter industryIDParam = new MySqlParameter("industry_id", industryId);
                    MySqlParameter saltParam = new MySqlParameter("salt", salt);


                    // Adds the parameter to the command
                    command.Parameters.Add(userNameParam);
                    command.Parameters.Add(hashParam);
                    command.Parameters.Add(ivParam);
                    command.Parameters.Add(publicKeyParam);
                    command.Parameters.Add(privateKeyParam);
                    command.Parameters.Add(industryIDParam);
                    command.Parameters.Add(saltParam);

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

        public string GetPrivateKey(string industryName)
        {
            //Calls another prepared statement to get the industry ID from the industry name
            int industryId = GetIndustryID(industryName);

            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(
                Env.GetString("OTHER_READER_NAME"),
                Env.GetString("OTHER_READER_PASSWORD")
            );

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    // Query to get private_key based on industry_id
                    string privateKeyQuery =
                        "SELECT private_key FROM regulators WHERE industry_id = @industry_id";

                    // Create and prepare an SQL statement for private_key
                    MySqlCommand privateKeyCommand = new MySqlCommand(privateKeyQuery, connection);
                    privateKeyCommand.Parameters.AddWithValue("industry_id", industryId);
                    privateKeyCommand.Prepare();

                    // Execute the query to get private_key
                    string privateKey = (string)privateKeyCommand.ExecuteScalar();

                    Console.WriteLine($"Encrypted Privatekey taken from database: {(privateKey)}");

                    Console.WriteLine($"Encrypted Privatekey taken from database: {(privateKey)}");

                    return privateKey;
                }
                catch (MySqlException ex)
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
            dbConnection.SetConnectionCredentials(
                Env.GetString("OTHER_READER_NAME"),
                Env.GetString("OTHER_READER_PASSWORD")
            );

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

        public int GetIndustryID(string industryName)
        {
            try
            {
                // Set credentials for the user needed
                dbConnection.SetConnectionCredentials(
                    Env.GetString("OTHER_READER_NAME"),
                    Env.GetString("OTHER_READER_PASSWORD")
                );

                // Use MySqlConnection to open the connection and throw an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    Console.WriteLine("Connection opened successfully.");

                    try
                    {
                        // Query to get industry_id based on industryName
                        string industryIdQuery =
                            "SELECT industry_id FROM industry WHERE industry_name = @industry_name";

                        // Create and prepare an SQL statement for industry_id
                        MySqlCommand industryIdCommand = new MySqlCommand(
                            industryIdQuery,
                            connection
                        );

                        industryIdCommand.Parameters.AddWithValue("@industry_name", industryName);

                        industryIdCommand.Prepare();

                        // Execute the query to get industry_id
                        int industryId = Convert.ToInt32(industryIdCommand.ExecuteScalar());

                        return industryId;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Error preparing command: {ex.Message}");
                        throw;
                    }
                    catch (MySqlException ex)
                    {
                        // Handle the exception (e.g., log it) and rethrow
                        Console.WriteLine($"Error executing query: {ex.Message}");
                        throw; // Rethrow the caught exception
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
                throw; // Rethrow the caught exception
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
            //Calls another prepared statement to get the industry ID from the industry name
            DotNetEnv.Env.Load();
            int industryId = GetIndustryID(report.IndustryName);

            try
            {
                // Set credentials for the user needed
                dbConnection.SetConnectionCredentials(
                    Env.GetString("REPORT_WRITER_NAME"),
                    Env.GetString("REPORT_WRITER_PASSWORD")
                );
                // Use mySqlConnection to open the connection and throw an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    try
                    {
                        // Create an instance of MySqlCommand
                        MySqlCommand command = new MySqlCommand(null, connection);

                        // Create and prepare an SQL statement.
                        command.CommandText =
                            $"INSERT INTO reports (industry_id, company_name, company_iv, description, desc_iv, email) VALUES (@industry_id, @company_name, @company_iv, @description, @desc_iv, @email)";

                        // Sets mySQL parameters for the prepared statement
                        MySqlParameter industryIDParam = new MySqlParameter(
                            "industry_id",
                            industryId
                        );
                        MySqlParameter companyNameParam = new MySqlParameter(
                            "company_name",
                            report.CompanyName
                        );
                        MySqlParameter companyIvParam = new MySqlParameter(
                            "company_iv",
                            report.CompanyIv
                        );
                        MySqlParameter descriptionParam = new MySqlParameter(
                            "description",
                            report.Description
                        );
                        MySqlParameter descIvParam = new MySqlParameter(
                            "desc_iv",
                            report.DescriptionIv
                        );

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
                        command.Parameters.Add(companyIvParam);
                        command.Parameters.Add(descriptionParam);
                        command.Parameters.Add(descIvParam);
                        command.Parameters.Add(emailParam);

                        // Call Prepare after setting the Commandtext and Parameters.
                        command.Prepare();

                        // Execute the query
                        object result = command.ExecuteScalar();

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

            //Calls another prepared statement to get the industry ID from the industry name
            int industryId = GetIndustryID(industryName);
            Console.WriteLine($"Got industry_id: {industryId}");

            // Set credentials for the user needed
            dbConnection.SetConnectionCredentials(
                Env.GetString("REPORT_READER_NAME"),
                Env.GetString("REPORT_READER_PASSWORD")
            );

            // Use mySqlConnection to open the connection and throw an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    // Create an instance of MySqlCommand
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText = "SELECT * FROM reports WHERE industry_id = @industry_id";

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
                            string companyIv = reader.GetString("company_iv");
                            string description = reader.GetString("description");
                            string descIv = reader.GetString("desc_iv");
                            string email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email");

                            Report report = new Report(
                                reportID,
                                industryName,
                                companyName,
                                companyIv,
                                description,
                                descIv,
                                email
                            );
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

        public string GetPublicKey(string industryName)
        {
            try
            {
                //Calls another prepared statement to get the industry ID from the industry name
                int industryId = GetIndustryID(industryName);

                Console.WriteLine($"industry name is: {industryName}");
                // Set credentials for the user needed
                dbConnection.SetConnectionCredentials(
                    Env.GetString("OTHER_READER_NAME"),
                    Env.GetString("OTHER_READER_PASSWORD")
                );

                // Use MySqlConnection to open the connection and throw an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    Console.WriteLine("Connection opened successfully.");

                    try
                    {
                        // Create an instance of MySqlCommand
                        MySqlCommand command = new MySqlCommand(null, connection);

                        // Create and prepare an SQL statement.
                        command.CommandText =
                            $"SELECT public_key FROM regulators WHERE industry_id = @industry_id";

                        // Sets mySQL parameters for the prepared statement
                        MySqlParameter industryIDParam = new MySqlParameter(
                            "industry_id",
                            industryId
                        );

                        // Adds the parameters to the command
                        command.Parameters.Add(industryIDParam);

                        // Call Prepare after setting the Commandtext and Parameters.
                        command.Prepare();

                        // Execute the query and cast the result to a byte array
                        string result = command.ExecuteScalar() as string;
                        Console.WriteLine("Query executed successfully.");

                        // Return the byte array
                        return result;
                    }
                    catch (MySqlException ex)
                    {
                        // Return null if an exception is thrown, may want to implement secure logging
                        Console.WriteLine($"Error executing query: {ex.Message}");
                        throw ex;
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

        public bool IndustryMatch(string userName, string industryName)
        {
            //Calls another prepared statement to get the industry ID from the industry name
            int industryId = GetIndustryID(industryName);

            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(
                Env.GetString("OTHER_READER_NAME"),
                Env.GetString("OTHER_READER_PASSWORD")
            );

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    Console.WriteLine("setting sql statement");
                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT CASE WHEN EXISTS (SELECT 1 FROM regulators WHERE industry_id = @industry_id AND regulator_name = @userName) THEN 1 ELSE 0 END;";

                    Console.WriteLine("parameterizes");
                    // Sets MySQL parameters for the prepared statement
                    MySqlParameter industryIdParam = new MySqlParameter("industry_id", industryId);
                    MySqlParameter userNameParam = new MySqlParameter("userName", userName);
                    
                    Console.WriteLine("adds parameters");
                    // Adds the parameters to the command
                    command.Parameters.Add(industryIdParam);
                    command.Parameters.Add(userNameParam);

                    Console.WriteLine("preparing");
                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    Console.WriteLine("executing");
                    // Execute the query and cast the result to a long
                    long result = (long)command.ExecuteScalar();
                    Console.WriteLine(result);

                    // Convert the long result to boolean
                    bool match = result == 1;

                    Console.WriteLine("returning");
                    //returns true after everything is done.
                    return match;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw ex;
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        public (string, string) FindRegulatorIvFromIndustryName(string industryName)
        {
            //Calls another prepared statement to get the industry ID from the industry name
            int industryId = GetIndustryID(industryName);

            //Set credentials for the user needed
            dbConnection.SetConnectionCredentials(
                Env.GetString("OTHER_READER_NAME"),
                Env.GetString("OTHER_READER_PASSWORD")
            );

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    string iv = "";
                    string regulatorName = "";
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT iv, regulator_name FROM regulators WHERE industry_id = @industry_id";

                    // Sets MySQL parameters for the prepared statement
                    MySqlParameter industryIdParam = new MySqlParameter("industry_id", industryId);

                    // Adds the parameters to the command
                    command.Parameters.Add(industryIdParam);

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Check if there are rows in the result
                        if (reader.Read())
                        {
                            // Retrieve the "iv" and "regulator_name" column values as strings
                            iv = reader["iv"].ToString();
                            regulatorName = reader["regulator_name"].ToString();
                        }
                    }

                    // Returns the iv and regulator_name as a tuple
                    return (iv, regulatorName);

                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

            public string FindRegulatorSalt(string industryName) 
            {
                //Calls another prepared statement to get the industry ID from the industry name
                int industryId = GetIndustryID(industryName);

                //Set credentials for the user needed
                dbConnection.SetConnectionCredentials(
                    Env.GetString("OTHER_READER_NAME"),
                    Env.GetString("OTHER_READER_PASSWORD")
                );

                //uses mySqlConnection to open the connection and throws an exception if it fails
                using (MySqlConnection connection = dbConnection.OpenConnection())
                {
                    try
                    {
                        string salt = "";

                        //creates an instance of MySqlCommand, a method in the mysql library
                        MySqlCommand command = new MySqlCommand(null, connection);

                        // Create and prepare an SQL statement.
                        command.CommandText =
                            $"SELECT salt FROM regulators WHERE industry_id = @industry_id";

                        // Sets MySQL parameters for the prepared statement
                        MySqlParameter industryIdParam = new MySqlParameter("industry_id", industryId);

                        // Adds the parameters to the command
                        command.Parameters.Add(industryIdParam);

                        // Call Prepare after setting the Commandtext and Parameters.
                        command.Prepare();

                        // Execute the query
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if there are rows in the result
                            if (reader.Read())
                            {
                                // Retrieve the "iv" and "regulator_name" column values as strings
                                salt = reader["salt"].ToString();
                            }
                        }

                        // Returns the iv and regulator_name as a tuple
                        return (salt);

                    }
                    //executes at the end, no matter if it returned a value before or not
                    finally
                    {
                        //closes the connection at the VERY end
                        dbConnection.CloseConnection();
                    }
                }
            }   
    }
}