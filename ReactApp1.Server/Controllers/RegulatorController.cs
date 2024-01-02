using Microsoft.AspNetCore.Mvc;

namespace ReactApp1
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegulatorController : ControllerBase
    {
        private readonly Security security;

        public RegulatorController()
        {
            // Instantiate the UserFunctionality class when creating the controller
            this.security = new Security();
        }

        [HttpPost("createRegulator")]
        public IActionResult CreateRegulator([FromBody] RegulatorRequest regulatorRequest)
        {
            try
            {
                string userName = regulatorRequest.UserName;
                string hashedPassword = regulatorRequest.HashedPassword;
                string password = regulatorRequest.Password;
                string industryName = regulatorRequest.IndustryName;

                security.CreateRegulator(userName, hashedPassword, industryName);

                return Ok(new { Success = true, Message = "Regulator created successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
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

        [HttpGet("userPassword/{userName}")]
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
    }

    public class RegulatorRequest
    {
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string Password { get; set; }
        public string IndustryName { get; set; }
    }
}