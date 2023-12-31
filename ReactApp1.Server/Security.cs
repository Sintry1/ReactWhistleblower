using System.Security.Cryptography;
using System.Text;

namespace ReactApp1
{
    public class Security
    {
        //creates instance of PreparedStatements for calling prepared statements from the PreparedStatements class
        private PreparedStatements ps = PreparedStatements.CreateInstance();

        //Encrypts password using Bcrypt and posts the hashedpassword to "StoreHashAndUserName, which then saves the hashed password
        //and username to the database, this may need to be changed if we don't want to store usernames in plain texta
        //as it could leak who the reporter is. Maybe hash usernames too?
        public void HashPassword(string userName, string password)
        {
            string salt;

            //generates a salt with a work factor of 16
            salt = BCrypt.Net.BCrypt.GenerateSalt(16);

            //hashes the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            //stores username and password in DB, this can be removed, if we are using other services for login
            ps.StoreHashAndUserName(userName, hashedPassword);
        }

        //Function for verifying password using Bcrypt
        public bool VerifyPassword(string userName, string password)
        {
            //If statement to verify the user exists in the database
            if (ps.ExistingUser(userName))
            {
                //Gets the hashedpassword from database using user name
                String hashedPassword = ps.GetHashedPassword(userName);

                //Check if the hashedPassword matches the input password
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            }
            //returns false if user doesn't exist
            return false;
        }

        //Function for deriving a key from username and password
        public string DecriveKey(string userName, string password)
        {
            //passes username and password to verifyPassword and enters the if statement ONLY if VerifyPassword returns true
            if (VerifyPassword(userName, password))
            {

                //Sets a combinedSecret of password and username
                string combinedSecret = password + userName;

                //Generates a Salt using the password
                byte[] deterministicSalt = Encoding.UTF8.GetBytes(password);

                // Derive a consistent user-specific key from combinedSecret using Rfc2898DeriveBytes
                using (var pbkdf2 = new Rfc2898DeriveBytes(combinedSecret, deterministicSalt, 600000, HashAlgorithmName.SHA256))
                {
                    byte[] keyBytes = pbkdf2.GetBytes(32); // 256 bits key
                    string key = Convert.ToBase64String(keyBytes);
                    return key;
                }
            }
            return null;
        }
    }
}