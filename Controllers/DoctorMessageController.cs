using Healthcare.Dtos.doctor;
using Healthcare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/doctor-messages")]
    public class DoctorMessageController : ControllerBase
    {
        private readonly HealthcareSystemContext _context;

        public DoctorMessageController(HealthcareSystemContext context)
        {
            _context = context;
        }

        private int GetDoctorId()
        {
            var idClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) throw new Exception("Doctor ID not found in token");
            return int.Parse(idClaim.Value);
        }

        // ================================================================
        // POST api/doctor-messages/send
        // ================================================================
        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            try
            {
                var doctorId = GetDoctorId();

                // جيب التخصص تلقائي من الدكتور
                var doctor = await _context.Doctors
                    .Include(d => d.Specialty)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

                var message = new DoctorMessage
                {
                    DoctorId = doctorId,
                    MessageBody = req.MessageBody,
                    Sender = "Doctor",
                    SpecialtyId = doctor?.SpecialtyId,
                    CreatedAt = DateTime.Now
                };

                _context.DoctorMessages.Add(message);
                await _context.SaveChangesAsync();
                return Ok(new { message = "تم إرسال الرسالة بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // GET api/doctor-messages/my-messages
        // ================================================================
        [HttpGet("my-messages")]
        [Authorize]
        public async Task<IActionResult> GetMyMessages()
        {
            try
            {
                var doctorId = GetDoctorId();
                var messages = await _context.DoctorMessages
                    .Where(m => m.DoctorId == doctorId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();

                var result = messages.Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    DoctorId = m.DoctorId,

                    MessageBody = m.MessageBody,
                    Sender = m.Sender,
                    CreatedAt = m.CreatedAt.HasValue
                        ? m.CreatedAt.Value.ToString("yyyy-MM-dd") : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}