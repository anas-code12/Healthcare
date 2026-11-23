using Healthcare.Dtos.Patient;
using Healthcare.Services;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/auth/patient")]
    public class PatientAuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public PatientAuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] PatientRegisterRequest req) 
        {
            var result = await _auth.RegisterPatientAsync(req);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PatientLoginRequest req)
        {
            var result = await _auth.LoginPatientAsync(req);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
    }
}