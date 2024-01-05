using System.Security.Cryptography;

namespace ReactApp1
{
    public class UserFunctionality
    {

        private PreparedStatements ps = PreparedStatements.CreateInstance();
        private Security security = new Security();
        public bool SendReport(string industryName, string companyName, string description, string email)
        {
            try
            {
                // Calls the GetPublicKey prepared statement and gets the byte array
                byte[] serializedPublicKey = ps.GetPublicKey(industryName);
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
                Console.WriteLine($"companyName encrypting: {encryptedCompanyName}");

                Console.WriteLine($"encrypting description: {description}");
                string encryptedDescription = security.Encrypt(description, publicKey);
                string encryptedEmail = security.Encrypt(email, publicKey);
                Console.WriteLine("Encrypted report fields.");

                Report reportToSend = new Report(industryName, encryptedCompanyName, encryptedDescription, encryptedEmail);

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


        public List<Report> RetrieveReports(string industryName)
        {
            try
            {
                // Retrieve the list of encrypted reports from the database
                List<Report> encryptedReports = ps.GetAllReportsByIndustryName(industryName);

                // Check if there are any encrypted reports
                if (encryptedReports.Count > 0)
                {
                    // Get the private key for decryption
                    byte[] serializedPrivateKey = ps.GetPrivateKey(industryName);

                    RSAParameters privateKey = Security.DeserializeRSAParameters(serializedPrivateKey);

                    // Create a list to store decrypted reports
                    List<Report> decryptedReports = new List<Report>();

                    // Decrypt each encrypted report and add it to the decryptedReports list
                    foreach (Report encryptedReport in encryptedReports)
                    {
                        string decryptedCompanyName = security.Decrypt(encryptedReport.CompanyName, privateKey);
                        string decryptedDescription = security.Decrypt(encryptedReport.Description, privateKey);
                        string decryptedEmail = encryptedReport.Email != null ? security.Decrypt(encryptedReport.Email, privateKey) : null;

                        Report decryptedReport = new Report(industryName, decryptedCompanyName, decryptedDescription, decryptedEmail);
                        decryptedReports.Add(decryptedReport);
                    }

                    // Return the list of decrypted reports
                    return decryptedReports;
                }
                else
                {
                    // Return an empty list if there are no encrypted reports
                    return new List<Report>();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it) and return an empty list or null
                // You may want to implement secure logging to store the error message
                return new List<Report>();
            }
        }
    }
}