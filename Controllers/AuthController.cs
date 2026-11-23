using Healthcare.Dtos.doctor;
using Healthcare.Services;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/auth/doctor")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] DoctorRegisterRequest req)
    {
        var result = await _auth.RegisterDoctorAsync(req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DoctorLoginRequest req)
    {
        var result = await _auth.LoginDoctorAsync(req);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}