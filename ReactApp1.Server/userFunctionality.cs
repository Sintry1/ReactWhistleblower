using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ReactApp1
{
    public class UserFunctionality
    {

        private PreparedStatements ps = PreparedStatements.CreateInstance();
        private Security security = new Security();

        public bool SendReport(string industryName, string companyName, string companyIv, string description, string descriptionIv, string email)
        {
            try
            {
                // Calls the GetPublicKey prepared statement and gets the byte array
                string serializedPublicKey = ps.GetPublicKey(industryName);
                Console.WriteLine($"Got public key from the database.: {serializedPublicKey}");

                // Calls DeserializeRSAParameters, to turn the byte array back into an RSAParameter
                RSAParameters publicKey = Security.DeserializeRSAParameters(serializedPublicKey);
                Console.WriteLine("Deserialized public key.");
                Console.WriteLine($"Deserialized public key: {publicKey}");
                Console.WriteLine($"  Modulus: {BitConverter.ToString(publicKey.Modulus)}");
                Console.WriteLine($"  Exponent: {BitConverter.ToString(publicKey.Exponent)}");

                // Encrypts each field with the RSAParameter
                Console.WriteLine($"encrypting companyName: {companyName}");
                string encryptedCompanyName = security.Encrypt(companyName, publicKey);
                Console.WriteLine($"companyName encrypted: {encryptedCompanyName}");

                Console.WriteLine($"encrypting description: {description}");
                string encryptedDescription = security.Encrypt(description, publicKey);
                Console.WriteLine($"encrypted description: {encryptedDescription}");

                string encryptedEmail="";
                if (!string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"encrypting email: {email}");
                    encryptedEmail = security.Encrypt(email, publicKey);
                    Console.WriteLine($"encrypted email: {encryptedEmail}");
                }

                Report reportToSend = new Report(null,industryName, encryptedCompanyName, companyIv, encryptedDescription, descriptionIv, encryptedEmail);

                // Sends the information to StoreReport and returns true if successful
                if (ps.StoreReport(reportToSend))
                {
                    Console.WriteLine("Report stored successfully.");
                    return true;
                }
                else
                {
                    // Returns false if it failed to store the report
                    Console.WriteLine("Failed to store the report.");
                    return false;
                }
            }
            // Returns false if ANY exception is thrown
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        public List<Report> RetrieveReports(string industryName, string userName)
        {
            try
            {
                // Retrieve the list of encrypted reports from the database
                List<Report> encryptedReports = ps.GetAllReportsByIndustryName(industryName);
                Console.WriteLine($"list of encrypted reports: {encryptedReports}");
                // Check if there are any encrypted reports
                if (encryptedReports.Count > 0)
                {
                    Console.WriteLine($"Number of reports fetched: {encryptedReports.Count}");

                    // Get the private key for decryption
                    Console.WriteLine($"fetching private key");
                    string encryptedSerializedPrivateKey = ps.GetPrivateKey(industryName);
                    Console.WriteLine($"fetched key: {(encryptedSerializedPrivateKey)}");

                    Console.WriteLine($"fetching hashed password");
                    //Fetches the password for key deriviation
                    string hashedPassword = ps.GetHashedPassword(userName);

                    Console.WriteLine($"Derives key from username and password");
                    //Derives key
                    string derivedKey = security.KeyDeriverForEncryptionAndDecryptionOfPrivateKey(userName,hashedPassword);
                    Console.WriteLine($"derived key: {(derivedKey)}");

                    (string iv, string username) = security.FindRegulatorIvFromIndustryName(industryName);

                    Console.WriteLine($"decrypts private key using derived key");
                    //uses derived key to decrypt the encrypted serialized private key
                    string serializedPrivateKey = Security.DecryptKey(encryptedSerializedPrivateKey, derivedKey,iv);
                    Console.WriteLine($"Got public key from the database.: {(serializedPrivateKey)}");


                    Console.WriteLine($"deserializes RSA parameters");
                    //deserializes it
                    RSAParameters privateKey = Security.DeserializeRSAParameters(serializedPrivateKey);
                    Console.WriteLine($"  Modulus: {BitConverter.ToString(privateKey.Modulus)}");
                    Console.WriteLine($"  Exponent: {BitConverter.ToString(privateKey.Exponent)}");

                    // Create a list to store decrypted reports
                    List<Report> decryptedReports = new List<Report>();

                    // Decrypt each encrypted report and add it to the decryptedReports list
                    Console.WriteLine($"entering decryption loop");
                    foreach (Report encryptedReport in encryptedReports)
                    {
                        Console.WriteLine($"decrypting company name");
                        //Decrypts companyName
                        string decryptedCompanyName = security.Decrypt(encryptedReport.CompanyName, privateKey);
                        Console.WriteLine($"decrypted company name: {decryptedCompanyName}");

                        Console.WriteLine($"decrypting Description");
                        //Decrypts description
                        string decryptedDescription = security.Decrypt(encryptedReport.Description, privateKey);
                        Console.WriteLine($"decrypted Description: {decryptedDescription}");

                        Console.WriteLine($"decrypting email if not null");
                        //Decrypts Email if not null
                        string decryptedEmail = encryptedReport.Email != null ? security.Decrypt(encryptedReport.Email, privateKey) : null;

                        Console.WriteLine($"creating decrypted report");
                        //Creates report object with the decrypted parameters
                        Report decryptedReport = new Report(encryptedReport.ReportID, industryName, decryptedCompanyName,encryptedReport.CompanyIv, decryptedDescription,encryptedReport.DescriptionIv, decryptedEmail);

                        Console.WriteLine($"Adding to decrypted report list");
                        //Adds the decrypted report to the decrypted report LIST
                        decryptedReports.Add(decryptedReport);
                    }

                    // Return the list of decrypted reports
                    return decryptedReports;
                }
                else
                {
                    // Return an empty list if there are no encrypted reports
                    throw new Exception("List is 0/empty");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it) and return an empty list or null
                // You may want to implement secure logging to store the error message
                Console.WriteLine(ex);
                throw ex;
            }
        }
    }
}