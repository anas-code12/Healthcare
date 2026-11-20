using Healthcare.Dtos.Patient;
using Healthcare.Services.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/patient")]
    public class PatientController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientController(PatientService patientService)
        {
            _patientService = patientService;
        }

        // ================================================================
        // POST api/patient/sick-record
        // ================================================================
        [HttpPost("sick-record")]
        public async Task<IActionResult> UpsertSickRecord([FromBody] PatientSickRecordUpsertRequest req)
        {
            var result = await _patientService.UpsertSickRecordAsync(req);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ================================================================
        // PUT api/patient/update-profile
        // المريض يعدل بياناته - محتاج Login
        // ================================================================
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdatePatientProfileRequest? req)
        {
            try
            {
                var patientId = int.Parse(
    User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

                var result = await _patientService.UpdatePatientProfileAsync(patientId, req);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}