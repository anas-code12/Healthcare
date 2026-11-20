using Healthcare.Dtos.Appointments;
using Healthcare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Healthcare.Dtos.Appointments.CompleteAppointmentRequest;

namespace Healthcare.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AppointmentService _service;

    public AppointmentController(AppointmentService service)
    {
        _service = service;
    }

    private int GetUserIdFromToken()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                   ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                   ?? User.FindFirst("sub");

        if (idClaim == null)
            throw new Exception("User ID not found in token");

        return int.Parse(idClaim.Value);
    }

    [HttpGet("doctor-availability/{doctorId}/{clinicId}")]
    public async Task<IActionResult> GetDoctorAvailability(int doctorId, int clinicId)
    {
        try
        {
            var result = await _service.GetDoctorAvailabilityAsync(doctorId, clinicId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("book")]
    [Authorize]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        try
        {
            var patientId = GetUserIdFromToken();
            var result = await _service.BookAppointmentAsync(patientId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-appointments")]
    [Authorize]
    public async Task<IActionResult> GetMyAppointments()
    {
        try
        {
            var patientId = GetUserIdFromToken();
            var result = await _service.GetPatientAppointmentsAsync(patientId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor-appointments")]
    [Authorize]
    public async Task<IActionResult> GetDoctorAppointments()
    {
        try
        {
            var doctorId = GetUserIdFromToken();
            var result = await _service.GetDoctorAppointmentsAsync(doctorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("cancel/{appointmentId}")]
    [Authorize]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        try
        {
            var patientId = GetUserIdFromToken();
            await _service.CancelAppointmentAsync(appointmentId, patientId);
            return Ok(new { message = "تم إلغاء الموعد بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-diseases-and-medicines")]
    [Authorize]
    public async Task<IActionResult> GetMyDiseasesAndMedicines()
    {
        try
        {
            var doctorId = GetUserIdFromToken();
            var result = await _service.GetDiseasesAndMedicinesByDoctorAsync(doctorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("complete/{appointmentId}")]
    [Authorize]
    public async Task<IActionResult> CompleteAppointment(int appointmentId,
        [FromBody] CompleteAppointmentRequest req)
    {
        try
        {
            var doctorId = GetUserIdFromToken();
            await _service.CompleteAppointmentAsync(appointmentId, doctorId, req);
            return Ok(new { message = "تم إنهاء الكشف بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("pending-diagnosis")]
    public async Task<IActionResult> GetPendingDiagnosis()
    {
        try
        {
            var result = await _service.GetPendingDiagnosisAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("save-ai-diagnosis")]
    public async Task<IActionResult> SaveAiDiagnosis([FromBody] AiDiagnosisRequest req)
    {
        try
        {
            await _service.SaveAiDiagnosisAsync(req);
            return Ok(new { message = "تم حفظ التشخيص بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("health-ministry")]
    public async Task<IActionResult> GetHealthMinistryData()
    {
        try
        {
            var result = await _service.GetHealthMinistryDataAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("chronic-disease-stats")]
    public async Task<IActionResult> GetChronicDiseaseStats()
    {
        try
        {
            var result = await _service.GetChronicDiseaseStatsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("rate/{appointmentId}")]
    [Authorize]
    public async Task<IActionResult> RateAppointment(int appointmentId, [FromBody] RateAppointmentRequest req)
    {
        try
        {
            var patientId = GetUserIdFromToken();
            await _service.RateAppointmentAsync(appointmentId, patientId, req.Rating);
            return Ok(new { message = "تم التقييم بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("patient-medical-history/{patientId}")]
    [Authorize]
    public async Task<IActionResult> GetPatientMedicalHistory(int patientId)
    {
        try
        {
            var result = await _service.GetPatientMedicalHistoryAsync(patientId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}