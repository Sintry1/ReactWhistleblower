using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;

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
        public void CreateRegulator(string userName, string hashedPassword, string industryName, string iv, string salt)
        {
            // Calls HashPassword with the password and sets the hashed password to the value returned
            // this hashedpassword is saved with other regulator information
            // var hashedPassword = HashPassword(password);

            // Generate key pair
            var keyPair = GenerateKeyPair();
            RSAParameters publicKey = keyPair.Item1;
            //Console.WriteLine($"  Modulus: {BitConverter.ToString(publicKey.Modulus)}");
            //Console.WriteLine($"  Exponent: {BitConverter.ToString(publicKey.Exponent)}");

            RSAParameters privateKey = keyPair.Item2;

            // Serialize RSA parameters
            string serializedPublicKey = SerializeRSAParameters(publicKey);
            //Console.WriteLine($"key serialized {BitConverter.ToString(serializedPublicKey)}");
            Console.WriteLine($"private key modulus: {BitConverter.ToString(privateKey.Modulus)}");
            Console.WriteLine($"private key Exponent: {BitConverter.ToString(privateKey.Exponent)}");
            Console.WriteLine($"private key DP: {BitConverter.ToString(privateKey.DP)}");
            Console.WriteLine($"private key DQ: {BitConverter.ToString(privateKey.DQ)}");
            Console.WriteLine($"private key Q: {BitConverter.ToString(privateKey.Q)}");
            Console.WriteLine($"private key D: {BitConverter.ToString(privateKey.D)}");
            Console.WriteLine($"private key InverseQ: {BitConverter.ToString(privateKey.InverseQ)}");
            Console.WriteLine($"private key P: {BitConverter.ToString(privateKey.P)}");


            string serializedPrivateKey = SerializeRSAParameters(privateKey);
            Console.WriteLine($"private key serialized: {(serializedPrivateKey)}");

            // gets a byte array as encryption key, using the called function
            string encryptionkey = KeyDeriverForEncryptionAndDecryptionOfPrivateKey(userName, hashedPassword);

            string encryptedSerializedPrivateKey = EncryptKey(serializedPrivateKey, encryptionkey,iv);
            Console.WriteLine($"private key serialized and encrypted: {encryptedSerializedPrivateKey}");

            // stores username and password in DB, this can be removed if we are using other services for login
            ps.StoreRegulatorInformation(userName, hashedPassword, serializedPublicKey, encryptedSerializedPrivateKey, industryName, iv, salt);
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
            Console.WriteLine($"username given by controller to C#: {userName}");
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
        public string KeyDeriverForEncryptionAndDecryptionOfPrivateKey(string userName, string password)
        {
            // Sets a combinedSecret of password and username
            string combinedSecret = password + userName;

            // Generates a Salt using the password
            byte[] deterministicSalt = Encoding.UTF8.GetBytes(userName);

            // Derive a consistent user-specific key from combinedSecret using Rfc2898DeriveBytes
            using (var pbkdf2 = new Rfc2898DeriveBytes(combinedSecret, deterministicSalt, 600000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(32)); // 256 bits key
            }
        }

        public static string EncryptKey(string keyToEncrypt, string encryptionKey, string iv)
        {
            // Creates an instance of the AES algorithm
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(encryptionKey);
                aesAlg.IV = Convert.FromBase64String(iv);
                aesAlg.Mode = CipherMode.CFB;

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(keyToEncrypt);
                            }
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // Decrypts a key using AES algorithm and a specified decryption key
        public static string DecryptKey(string encryptedKey, string decryptionKey, string iv)
        {
            // Creates an instance of the AES algorithm
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(decryptionKey);
                aesAlg.IV = Convert.FromBase64String(iv);
                aesAlg.Mode = CipherMode.CFB;

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedKey)))
                {
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
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
        // Serializes the RSA parameters into a byte array for storage
        public static string SerializeRSAParameters(RSAParameters parameters)
        {
            // Create a custom object for serialization
            var serializedParameters = new SerializedRSAParameters
            {
                Modulus = parameters.Modulus,
                Exponent = parameters.Exponent,
                D = parameters.D,
                DP = parameters.DP,
                DQ = parameters.DQ,
                InverseQ = parameters.InverseQ,
                P = parameters.P,
                Q = parameters.Q
            };

            // Serialize the custom object to JSON string
            string jsonString = JsonConvert.SerializeObject(serializedParameters);
            return jsonString;
        }

        public static RSAParameters DeserializeRSAParameters(string serializedParameters)
        {
            // Deserialize the JSON string to the custom object
            var deserializedParameters = JsonConvert.DeserializeObject<SerializedRSAParameters>(serializedParameters);

            // Convert the custom object back to RSAParameters
            RSAParameters rsaParameters = new RSAParameters
            {
                Modulus = deserializedParameters.Modulus,
                Exponent = deserializedParameters.Exponent,
                D = deserializedParameters.D,
                DP = deserializedParameters.DP,
                DQ = deserializedParameters.DQ,
                InverseQ = deserializedParameters.InverseQ,
                P = deserializedParameters.P,
                Q = deserializedParameters.Q
            };

            return rsaParameters;
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
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {

                    Console.WriteLine($"Original Private Key Modulus: {BitConverter.ToString(privateKey.Modulus)}");
                    Console.WriteLine($"Original Private Key Exponent: {BitConverter.ToString(privateKey.Exponent)}");

                    rsa.ImportParameters(privateKey);

                    Console.WriteLine($"Imported Private Key Modulus: {BitConverter.ToString(rsa.ExportParameters(true).Modulus)}");
                    Console.WriteLine($"Imported Private Key Exponent: {BitConverter.ToString(rsa.ExportParameters(true).Exponent)}");

                    byte[] encryptedData = Convert.FromBase64String(encryptedText);
                    Console.WriteLine($"Encrypted Data: {BitConverter.ToString(encryptedData)}");


                    byte[] decryptedBytes = rsa.Decrypt(encryptedData, true);
                    Console.WriteLine($"Decrypted Data: {BitConverter.ToString(decryptedBytes)}");

                    Console.WriteLine($"Decrypted Data from decrypt in security: {BitConverter.ToString(decryptedBytes)}");


                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"CryptographicException during decryption: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during decryption: {ex.Message}");
                throw;
            }
        }

        //Fetches regulator IV and name from database
        public (string, string) FindRegulatorIvFromIndustryName(string industryName)
        {
            try
            {
                return ps.FindRegulatorIvFromIndustryName(industryName);

            } catch (Exception e)
            {
                throw e;
            }
        }

        //Fetches regulator salt from database
        public string FindRegulatorSalt(string industryName)
        {
            try
            {
                return ps.FindRegulatorSalt(industryName);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
