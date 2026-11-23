using Healthcare.Dtos.doctor;
using Healthcare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/ministry")]
    public class MinistryController : ControllerBase
    {
        private readonly IMinistryService _service;

        public MinistryController(IMinistryService service)
        {
            _service = service;
        }

        // ================================================================
        // POST api/ministry/login
        // ================================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MinistryLoginRequest req)
        {
            try
            {
                var result = await _service.LoginAsync(req);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // ================================================================
        // GET api/ministry/messages
        // ================================================================
        [HttpGet("messages")]
        [Authorize]
        public async Task<IActionResult> GetMessages()
        {
            try
            {
                var result = await _service.GetAllMessagesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // POST api/ministry/send-message
        // وزارة الصحة تبعت رسالة لدكتور
        // ================================================================
        [HttpPost("send-message")]
        [Authorize]
        public async Task<IActionResult> SendMessageToDoctor([FromBody] MinistryToDocterMessageRequest req)
        {
            try
            {
                await _service.SendMessageToDoctorAsync(req);
                return Ok(new { message = "تم إرسال الرسالة للدكتور بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // PUT api/ministry/verify-doctor
        // ================================================================
        [HttpPut("verify-doctor")]
        [Authorize]
        public async Task<IActionResult> VerifyDoctor([FromBody] VerifyDoctorRequest req)
        {
            try
            {
                await _service.VerifyDoctorAsync(req);
                return Ok(new { message = "تم تحديث حالة الدكتور بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // POST api/ministry/add-disease
        // ================================================================
        [HttpPost("add-disease")]
        [Authorize]
        public async Task<IActionResult> AddDisease([FromBody] AddDiseaseRequest req)
        {
            try
            {
                await _service.AddDiseaseAsync(req);
                return Ok(new { message = "تم إضافة المرض بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================================================================
        // POST api/ministry/add-medicine
        // ================================================================
        [HttpPost("add-medicine")]
        [Authorize]
        public async Task<IActionResult> AddMedicine([FromBody] AddMedicineRequest req)
        {
            try
            {
                await _service.AddMedicineAsync(req);
                return Ok(new { message = "تم إضافة الدواء بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // ================================================================
        // GET api/ministry/doctors-licenses
        // وزارة الصحة تشوف الأرقام الوظيفية للدكاترة
        // ================================================================
        [HttpGet("doctors-licenses")]
 
        public async Task<IActionResult> GetDoctorsLicenses()
        {
            try
            {
                var result = await _service.GetDoctorsLicenseNumbersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}