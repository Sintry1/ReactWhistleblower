﻿namespace ReactApp1 { 
    public class Report
    {
        //int? allows it to be nullable
        public int? ReportID { get; set; }
        public string IndustryName { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string CompanyIv { get; set; }
        public string DescriptionIv { get; set; }


        // Constructor for reports
        public Report(int? reportId, string industryName, string companyName, string companyIv, string description, string descriptionIv, string email)
        {
            ReportID = reportId;
            IndustryName = industryName;
            CompanyName = companyName;
            CompanyIv = companyIv;
            Description = description;
            DescriptionIv = descriptionIv;
            Email = email;
        }
    }

}
