using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server;

namespace ReactApp1
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegulatorController : ControllerBase
    {
        private readonly Security security;

        public RegulatorController()
        {
            // Instantiate the Security class when creating the controller
            this.security = new Security();
        }

        [HttpPost("createRegulator")]
        public IActionResult CreateRegulator([FromBody] Regulator regulator)
        {
            try
            {
                string userName = regulator.UserName;
                string hashedPassword = regulator.HashedPassword;
                string industryName = regulator.IndustryName;

                security.CreateRegulator(userName, hashedPassword, industryName);

                return Ok(new { Success = true, Message = "Regulator created successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine("Exception message: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("userExists/{userName}")]
        public IActionResult UserExists(string userName)
        {
            try
            {
                bool exists = security.UserExists(userName);

                return Ok(new { Success = true, UserExists = exists });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("passwordCheck/{userName}")]
        public IActionResult UserPassword(string userName)
        {
            try
            {
                string hashedPassword = security.UserPassword(userName);

                return Ok(new { Success = true, HashedPassword = hashedPassword });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("checkIndustry/{userName}/{industryName}")]
        public IActionResult IndustryMatch(string userName, string industryName)
        {
            try
            {
                bool exists = security.IndustryMatch(userName, industryName);

                return Ok(new { Success = true, IndustryMatch = exists });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }
    }

    public class RegulatorRequest
    {
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string Password { get; set; }
        public string IndustryName { get; set; }
    }
}
