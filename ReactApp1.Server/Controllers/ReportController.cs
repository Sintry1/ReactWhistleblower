using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ReactApp1
{
    [Route("api/[controller]")]
    [ApiController]
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
                string description = reportRequest.Description;
                string email = reportRequest.Email;

                bool result = userFunctionality.SendReport(industryName, companyName, description, email);

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

        [HttpGet("retrieveReports/{industryName}")]
        public IActionResult RetrieveReports(string industryName)
        {
            try
            {
                List<Report> reports = userFunctionality.RetrieveReports(industryName);

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
