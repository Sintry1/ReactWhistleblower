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
            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT CASE WHEN EXISTS (SELECT 1 FROM userTable WHERE userName = @userName) THEN CAST('TRUE' AS BIT) ELSE CAST('FALSE' AS BIT) END";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("@userName", userName);

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
        public void StoreHashAndUserName(string userName, string hash)
        {

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"INSERT INTO userTable (username, hash) VALUES (@userName, @hash)";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("@userName", userName);

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter hashParam = new MySqlParameter("@hash", hash);

                    // Adds the parameter to the command
                    command.Parameters.Add(userNameParam);

                    // Adds the parameter to the command
                    command.Parameters.Add(hashParam);

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

        //Gets the stores has of the user's password, this method may be COMPLETELY obsolete, thanks to firebase
        public string GetHashedPassword(string userName)
        {
            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {

                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT hash FROM userTable WHERE userName = @userName";

                    // Sets a mySQL parameter for the prepared statement
                    MySqlParameter userNameParam = new MySqlParameter("@userName", userName);

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

        //Stores message, may need to be modified if we need userID or other things stored with the message
        public bool StoreMessage(string msg, string sessionKey, string publicKey)
        {

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"INSERT INTO report (message) VALUES (@msg,@sessionKey, @publicKey)";

                    // Sets mySQL parameters for the prepared statement
                    MySqlParameter msgParam = new MySqlParameter("@msg", msg);
                    MySqlParameter sessionKeyParam = new MySqlParameter("@sessionKey", sessionKey);
                    MySqlParameter publicKeyParam = new MySqlParameter("@publicKey", publicKey);

                    // Adds the parameters to the command
                    command.Parameters.Add(msgParam);
                    command.Parameters.Add(sessionKeyParam);
                    command.Parameters.Add(publicKeyParam);

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query and cast the result to a boolean
                    command.ExecuteNonQuery();

                    //return true if no exceptions are thrown
                    return true;
                }
                catch (MySqlException ex)
                {
                    //return false if exception is thrown, may want to implement secure logging if we can to store the error message
                    return false;
                }
                //executes at the end, no matter if it returned a value before or not
                finally
                {
                    //closes the connection at the VERY end
                    dbConnection.CloseConnection();
                }
            }
        }

        public List<string> GetAllPublicKeys()
        {
            //Empty list to fill with keys
            List<string> keyEntries = new List<string>();
            string columnName = "PublicKeys";

            //uses mySqlConnection to open the connection and throws an exception if it fails
            using (MySqlConnection connection = dbConnection.OpenConnection())
            {
                try
                {
                    //creates an instance of MySqlCommand, a method in the mysql library
                    MySqlCommand command = new MySqlCommand(null, connection);

                    // Create and prepare an SQL statement.
                    command.CommandText =
                        $"SELECT PublicKeys FROM KeyList";

                    // Call Prepare after setting the Commandtext and Parameters.
                    command.Prepare();

                    // Execute the query
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Assuming the column type is string, change accordingly based on the actual type
                            string entry = reader.GetString(columnName);
                            keyEntries.Add(entry);
                        }
                    }
                    return keyEntries;
                }
                catch (MySqlException ex)
                {
                    //return false if exception is thrown, may want to implement secure logging if we can to store the error message
                    return keyEntries;
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