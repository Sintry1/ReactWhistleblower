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
        public void CreateRegulator(string userName, string password, int industryID)
        {
            //Calls HashPassword with the password and sets the hashed password to the value returned
            //this hashedpassword is saved with other regulator information
            var hashedPassword = HashPassword(password);

            // Generate key pair
            var keyPair = GenerateKeyPair();
            byte[] publicKey = keyPair.Item1;
            byte[] privateKey = keyPair.Item2;

            //gets a byte array as encryption key, using the called function
            byte[] encryptionkey = KeyDeriverForEncryptionAndDecryptionOfPrivateKey(userName, password);

            byte[] encryptedPrivateKey = EncryptKey(privateKey, encryptionkey);

            //stores username and password in DB, this can be removed, if we are using other services for login
            ps.StoreRegulatorInformation(userName, hashedPassword, publicKey, encryptedPrivateKey, industryID);
        }

        private string HashPassword(string password)
        {
            string salt;

            //generates a salt with a work factor of 16
            salt = BCrypt.Net.BCrypt.GenerateSalt(16);

            //hashes the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword;
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
        private string DecriveKey(string userName, string password)
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

        private byte[] KeyDeriverForEncryptionAndDecryptionOfPrivateKey(string userName, string password)
        {
            // Sets a combinedSecret of password and username
            string combinedSecret = password + userName;

            //Generates a Salt using the password
            byte[] deterministicSalt = Encoding.UTF8.GetBytes(userName);

            // Derive a consistent user-specific key from combinedSecret using Rfc2898DeriveBytes
            using (var pbkdf2 = new Rfc2898DeriveBytes(combinedSecret, deterministicSalt, 600000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256 bits key
            }
        }

        // Encrypt a key using a different key
        public static byte[] EncryptKey(byte[] keyToEncrypt, byte[] encryptionKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = encryptionKey;

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // Write the key to be encrypted to the stream
                                swEncrypt.Write(Encoding.UTF8.GetString(keyToEncrypt));
                            }
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        // Decrypt password using AES and the provided key and IV
        public static byte[] DecryptKey(byte[] encryptedKey, byte[] decryptionKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = decryptionKey;

                using (MemoryStream msDecrypt = new MemoryStream(encryptedKey))
                {
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted key from the stream
                                string decryptedKey = srDecrypt.ReadToEnd();
                                return Encoding.UTF8.GetBytes(decryptedKey);
                            }
                        }
                    }
                }
            }
        }

        public static Tuple<byte[], byte[]> GenerateKeyPair()
        {
            using (ECDiffieHellmanCng dh = new ECDiffieHellmanCng())
            {
                // Generate public-private key pair
                byte[] publicKey = dh.PublicKey.ToByteArray();

                // Export private key using key exchange format (not recommended for security reasons)
                byte[] privateKey = dh.Key.Export(CngKeyBlobFormat.EccPrivateBlob);

                return Tuple.Create(publicKey, privateKey);
            }
        }
    }
}