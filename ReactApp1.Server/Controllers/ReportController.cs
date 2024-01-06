using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ReactApp1
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly UserFunctionality userFunctionality;

        public ReportController()
        {
            // Instantiate the UserFunctionality class when creating the controller
            this.userFunctionality = new UserFunctionality();
        }

        [HttpPost("sendReport")]
        public IActionResult SendReport([FromBody] Report reportRequest)
        {
            try
            {
                string industryName = reportRequest.IndustryName;
                string companyName = reportRequest.CompanyName;
                string companyIv = reportRequest.CompanyIv;
                string description = reportRequest.Description;
                string descriptionIv = reportRequest.DescriptionIv;
                string email = reportRequest.Email;
                

                bool result = userFunctionality.SendReport(industryName, companyName, companyIv, description, descriptionIv, email);

                if (result)
                {
                    return Ok(new { Success = true, Message = "Report sent successfully." });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to send the report." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("getReports/{industryName}/{userName}")]
        public IActionResult RetrieveReports(string industryName, string userName)
        {
            try
            {
                List<Report> reports = userFunctionality.RetrieveReports(industryName, userName);

                return Ok(new { Success = true, Reports = reports });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }
    }
}
