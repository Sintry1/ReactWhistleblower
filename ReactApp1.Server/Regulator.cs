namespace ReactApp1.Server
{
    public class Regulator
    {
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string IndustryName { get; set; }

        // Constructor for regulators
        public Regulator(string userName, string hashedPassword, string industryName)
        {
            UserName = userName;
            HashedPassword = hashedPassword;
            IndustryName = industryName;
        }
    }
}
