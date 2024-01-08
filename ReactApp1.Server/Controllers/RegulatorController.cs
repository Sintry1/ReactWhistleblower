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
                string iv = regulator.Iv;
                string salt = regulator.Salt;

                security.CreateRegulator(userName, hashedPassword, industryName, iv, salt);

                return Ok(new { Success = true, Message = "Regulator created successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine("Exception message: " + ex.Message);
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("userExists")]
        public IActionResult UserExists()
        {
            try
            {
                string userName = Request.Headers["name-Header"].ToString();
                bool exists = security.UserExists(userName);

                return Ok(new { Success = true, UserExists = exists });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("passwordCheck")]
        public IActionResult UserPassword()
        {
            try
            {
                string userName = Request.Headers["name-Header"].ToString();
                string hashedPassword = security.UserPassword(userName);

                return Ok(new { Success = true, HashedPassword = hashedPassword });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("checkIndustry/{industryName}")]
        public IActionResult IndustryMatch( string industryName)
        {
            
            try
            {
                string userName = Request.Headers["name-Header"].ToString();
                bool exists = security.IndustryMatch(userName, industryName);

                return Ok(new { Success = true, IndustryMatch = exists });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("GetIvAndUserName/{industryName}")]
        public IActionResult FindIvFromRegulatorIndustryName(string industryName)
        {
            try
            {
                // Destructuring the tuple directly in the method signature
                (string iv, string userName) = security.FindRegulatorIvFromIndustryName(industryName);

                return Ok(new { Success = true, Iv = iv, UserName = userName });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }

        [HttpGet("GetRegulatorSalt/{industryName}")]
        public IActionResult FindRegulatorSalt(string industryName)
        {
            try
            {
                // Destructuring the tuple directly in the method signature
                string salt = security.FindRegulatorSalt(industryName);

                return Ok(new { Success = true, Salt = salt});
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                return StatusCode(500, new { Success = false, Message = "Internal server error." });
            }
        }
    }
}
