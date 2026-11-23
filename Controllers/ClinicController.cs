using Healthcare.Dtos.Clinics;
using Healthcare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Healthcare.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicController : ControllerBase
{
    private readonly IClinicService _service;

    public ClinicController(IClinicService service)
    {
        _service = service;
    }

    private int GetDoctorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                 ?? User.FindFirst("sub");

        if (claim == null) throw new Exception("Doctor ID not found in token");
        return int.Parse(claim.Value);
    }

    // ================================================================
    // GET api/Clinic/my-clinics
    // جيب عيادات الدكتور - محتاج Login
    // ================================================================
    [HttpGet("my-clinics")]
    
    public async Task<IActionResult> GetMyClinics()
    {
        try
        {
            var doctorId = GetDoctorId();
            var result = await _service.GetDoctorClinicsAsync(doctorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ================================================================
    // POST api/Clinic
    // إضافة عيادة جديدة - محتاج Login
    // Request body مثال:
    // {
    //   "clinicName": "عيادة الشفاء",
    //   "clinicPhone": "01012345678",
    //   "location": "Cairo",
    //   "workingDays": "Saturday,Sunday,Monday",
    //   "workStartTime": "09:00",
    //   "workEndTime": "17:00",
    //   "doctorPrice": 300,
    //   "slotDurationMinutes": 30
    // }
    // ================================================================
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddClinic([FromBody] ClinicUpsertRequest request)
    {
        try
        {
            var doctorId = GetDoctorId();
            var result = await _service.AddClinicAsync(doctorId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ================================================================
    // PUT api/Clinic/{clinicId}
    // تعديل عيادة - محتاج Login
    // ================================================================
    [HttpPut("{clinicId}")]
    [Authorize]
    public async Task<IActionResult> UpdateClinic(int clinicId, [FromBody] ClinicUpsertRequest request)
    {
        try
        {
            var doctorId = GetDoctorId();
            var result = await _service.UpdateClinicAsync(doctorId, clinicId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ================================================================
    // DELETE api/Clinic/{clinicId}
    // حذف عيادة - محتاج Login
    // ================================================================
    [HttpDelete("{clinicId}")]
    [Authorize]
    public async Task<IActionResult> DeleteClinic(int clinicId)
    {
        try
        {
            var doctorId = GetDoctorId();
            await _service.DeleteClinicAsync(doctorId, clinicId);
            return Ok(new { message = "تم حذف العيادة بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ================================================================
    // POST api/Clinic/{clinicId}/breaks
    // إضافة بريك - محتاج Login
    // Body: { "breakTime": "13:00" }
    // ================================================================
    [HttpPost("{clinicId}/breaks")]
    [Authorize]
    public async Task<IActionResult> AddBreak(int clinicId, [FromBody] AddBreakRequest request)
    {
        try
        {
            var doctorId = GetDoctorId();
            var result = await _service.AddBreakAsync(doctorId, clinicId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ================================================================
    // DELETE api/Clinic/{clinicId}/breaks/{breakTime}
    // حذف بريك - محتاج Login
    // مثال: DELETE api/Clinic/1/breaks/13:00
    // ================================================================
    [HttpDelete("{clinicId}/breaks/{breakTime}")]
    [Authorize]
    public async Task<IActionResult> DeleteBreak(int clinicId, string breakTime)
    {
        try
        {
            var doctorId = GetDoctorId();
            await _service.DeleteBreakAsync(doctorId, clinicId, breakTime);
            return Ok(new { message = "تم حذف البريك بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}