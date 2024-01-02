using System.Security.Cryptography;

namespace ReactApp1
{
    public class UserFunctionality
    {

        private PreparedStatements ps = PreparedStatements.CreateInstance();
        private Security security = new Security();
        public bool SendReport(string industryName,string companyName, string Description, string email)
        {
            
            try
            {
                //Calls the GetPublicKey prepared statement and gets the byte array
                byte[] serializedpublicKey = ps.GetPublicKey(industryName);

                //Calls DeserializeRSAParameters, to turn the byte array back into a RSAParamater
                RSAParameters publicKey = Security.DeserializeRSAParameters(serializedpublicKey);
                
                //encrypts each field with the RSAParamater
                string encryptedCompanyName = security.Encrypt(companyName, publicKey);
                string encryptedDescription = security.Encrypt(Description, publicKey);
                string encryptedEmail = security.Encrypt(email, publicKey);
                Report reportToSend = new Report(null, industryName, encryptedCompanyName, encryptedDescription, encryptedEmail);

                //Sends the information to StoreMessage and returns true if successful
                if (ps.StoreReport(reportToSend))
                {
                    return true;
                }
                else { 

                //returns fall if it failed to store the report
                return false;
                }
            }
            // returns false if ANY exception is thrown
            catch { return false; }
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

                        Report decryptedReport = new Report(encryptedReport.ReportID, industryName, decryptedCompanyName, decryptedDescription, decryptedEmail);
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