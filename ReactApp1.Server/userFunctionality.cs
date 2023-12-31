using System.Security.Cryptography;

namespace ReactApp1
{
    public class userFunctionality
    {

        private PreparedStatements ps = PreparedStatements.CreateInstance();
        public bool SendMessage(string msg, string key)
        {
            //Add if statement around this to verify the user exists in the database
            try
            {
                string publicKey = null;
                List<string> entries = ps.GetAllPublicKeys();
                if (entries != null) {
                    foreach (string entry in entries)
                    {
                        var sessionKey = RandomNumberGenerator.GetBytes(32);
                        

                        //Calls the store message method and hands userID, msg and key to it
                        ps.StoreMessage(msg, publicKey, key);

                    }

                    // returns true if successful
                    return true;
                }
                return false;
            }
            // returns false if ANY exception is thrown
            catch { return false; }
        }
    }
}