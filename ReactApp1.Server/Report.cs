namespace ReactApp1 { 
    public class Report
    {
        //int? allows it to be nullable
        public int? ReportID { get; set; }
        public string IndustryName { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string? Email { get; set; }

        // Constructor for reports
        public Report(string industryName, string companyName, string description, string? email)
        {
            ReportID = null;
            IndustryName = industryName;
            CompanyName = companyName;
            Description = description;
            Email = email;
        }
    }

}
