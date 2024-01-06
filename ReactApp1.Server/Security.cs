using System.Security.Cryptography;
using System.Text;

namespace ReactApp1
{
    public class Security
    {
        // creates instance of PreparedStatements for calling prepared statements from the PreparedStatements class
        private PreparedStatements ps = PreparedStatements.CreateInstance();

        /* Takes username, password and industry ID
         * Encrypts password using Bcrypt
         * calls a function that generates a public/private key pair for the regulator
         * serializes them into bytes so they can be stored.
         * calls a function that derives an encryption key
         * encrypts the private key with the derived key
         * Sends all of it to StoreRegulatorInformation, which then saves it in the database.
         */
        public void CreateRegulator(string userName, string hashedPassword, string industryName, string iv)
        {
            // Calls HashPassword with the password and sets the hashed password to the value returned
            // this hashedpassword is saved with other regulator information
            // var hashedPassword = HashPassword(password);

            // Generate key pair
            var keyPair = GenerateKeyPair();
            RSAParameters publicKey = keyPair.Item1;
            Console.WriteLine($"  Modulus: {BitConverter.ToString(publicKey.Modulus)}");
            Console.WriteLine($"  Exponent: {BitConverter.ToString(publicKey.Exponent)}");

            RSAParameters privateKey = keyPair.Item2;

            // Serialize RSA parameters
            byte[] serializedPublicKey = SerializeRSAParameters(publicKey);
            Console.WriteLine($"key serialized {BitConverter.ToString(serializedPublicKey)}");

            byte[] serializedPrivateKey = SerializeRSAParameters(privateKey);

            // gets a byte array as encryption key, using the called function
            byte[] encryptionkey = KeyDeriverForEncryptionAndDecryptionOfPrivateKey(userName, hashedPassword);

            byte[] encryptedSerializedPrivateKey = EncryptKey(serializedPrivateKey, encryptionkey);

            // stores username and password in DB, this can be removed if we are using other services for login
            ps.StoreRegulatorInformation(userName, hashedPassword, serializedPublicKey, encryptedSerializedPrivateKey, industryName, iv);
        }

        /*
       //Function for hashing password using bcrypt
       private string HashPassword(string password)
       {
           string salt;

           //generates a salt with a work factor of 16
           salt = BCrypt.Net.BCrypt.GenerateSalt(16);

           //hashes the password
           var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
           return hashedPassword;
       }
       */

        // Function for calling the preparedStatement and returning true if user exists
        public bool UserExists(string userName)
        {
            if (ps.ExistingUser(userName))
            {
                return true;
            }
            return false;
        }

        //Checks if industry matches the industry belonging to the user
        public bool IndustryMatch(string userName, string industryName)
        {
            if (ps.IndustryMatch(userName, industryName))
            {
                return true;
            }
            return false;
        }



        // Function for calling the preparedStatement and returning the hashedPassword
        public string UserPassword(string userName)
        {
            return ps.GetHashedPassword(userName);
        }

        //Function for verifying password using Bcrypt
        /*
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
        }*/

        
        // Function for deriving a key from username and password
        public byte[] KeyDeriverForEncryptionAndDecryptionOfPrivateKey(string userName, string password)
        {
            // Sets a combinedSecret of password and username
            string combinedSecret = password + userName;

            // Generates a Salt using the password
            byte[] deterministicSalt = Encoding.UTF8.GetBytes(userName);

            // Derive a consistent user-specific key from combinedSecret using Rfc2898DeriveBytes
            using (var pbkdf2 = new Rfc2898DeriveBytes(combinedSecret, deterministicSalt, 600000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256 bits key
            }
        }

        // Encrypt a key using a different key
        // Encrypts a key using AES algorithm and a specified encryption key
        public static byte[] EncryptKey(byte[] keyToEncrypt, byte[] encryptionKey)
        {
            // Creates an instance of the AES algorithm
            using (Aes aesAlg = Aes.Create())
            {
                // Sets the encryption key for the AES algorithm
                aesAlg.Key = encryptionKey;

                // Creates a memory stream to store the encrypted data
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // Creates an encryptor using the AES algorithm
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                    {
                        // Creates a CryptoStream to perform the encryption
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            // Creates a StreamWriter to write the key to be encrypted to the stream
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // Writes the key to be encrypted to the stream
                                swEncrypt.Write(Encoding.UTF8.GetString(keyToEncrypt));
                            }
                        }
                    }

                    // Converts the encrypted data in the memory stream to a byte array and returns it
                    return msEncrypt.ToArray();
                }
            }
        }

        // Decrypts a key using AES algorithm and a specified decryption key
        public static byte[] DecryptKey(byte[] encryptedKey, byte[] decryptionKey)
        {
            // Creates an instance of the AES algorithm
            using (Aes aesAlg = Aes.Create())
            {
                // Sets the decryption key for the AES algorithm
                aesAlg.Key = decryptionKey;

                // Creates a memory stream to store the encrypted key
                using (MemoryStream msDecrypt = new MemoryStream(encryptedKey))
                {
                    // Creates a decryptor using the AES algorithm
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                    {
                        // Creates a CryptoStream to perform the decryption
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            // Creates a StreamReader to read the decrypted key from the stream
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Reads the decrypted key from the stream
                                string decryptedKey = srDecrypt.ReadToEnd();

                                // Converts the decrypted key to a byte array and returns it
                                return Encoding.UTF8.GetBytes(decryptedKey);
                            }
                        }
                    }
                }
            }
        }

        // Generates a key pair for the regulator and returns them as byte arrays
        public static Tuple<RSAParameters, RSAParameters> GenerateKeyPair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // Generate public-private key pair
                RSAParameters publicKey = rsa.ExportParameters(false);
                RSAParameters privateKey = rsa.ExportParameters(true);

                return Tuple.Create(publicKey, privateKey);
            }
        }

        // Serializes the RSA parameters into a byte array for storage
        private byte[] SerializeRSAParameters(RSAParameters parameters)
        {
            // Uses System.Text.Json for serialization
            string jsonString = System.Text.Json.JsonSerializer.Serialize(new RSAParametersJson
            {
                Modulus = parameters.Modulus,
                Exponent = parameters.Exponent
            });
            return Encoding.UTF8.GetBytes(jsonString);
        }

        // Deserializes the byte array back into RSAParameter
        public static RSAParameters DeserializeRSAParameters(byte[] serializedParameters)
        {
            // Uses System.Text.Json for deserialization
            string jsonString = Encoding.UTF8.GetString(serializedParameters);
            return System.Text.Json.JsonSerializer.Deserialize<RSAParametersJson>(jsonString).ToRSAParameters();
        }

        // Encrypts the msg using the publicKey of the regulator with RSA
        public string Encrypt(string msg, RSAParameters publicKey)
        {
            Console.WriteLine($"message to encrypt: {msg}");
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                Console.WriteLine($"Importing publickey parameters");
                rsa.ImportParameters(publicKey);

                Console.WriteLine($"Converts string to byte");
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(msg);
                Console.WriteLine($"message to encrypt: {plaintextBytes}");

                Console.WriteLine($"encrypting using RSA");
                byte[] encryptedData = rsa.Encrypt(plaintextBytes, true);

                return Convert.ToBase64String(encryptedData);
            }
        }

        // Decrypts the encrypted message using the private key with RSA
        public string Decrypt(string encryptedText, RSAParameters privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);

                byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = rsa.Decrypt(encryptedData, true);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        //Checks if industry matches the industry belonging to the user
        public string FindIvFromIndustryName(string industryName)
        {
            try
            {
                return ps.FindIvFromIndustryName(industryName);

            } catch (Exception e)
            {
                throw e;
            }
        }
    }
}
