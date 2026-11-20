using Healthcare.Dtos.patient;
using Healthcare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientProfileController : ControllerBase
    {
        private readonly HealthcareSystemContext _context;

        public PatientProfileController(HealthcareSystemContext context)
        {
            _context = context;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetProfile(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.PatientSickRecord) // ✅ بنجيب الـ SickRecord مع الـ Patient
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

         
           
            var sick = patient.PatientSickRecord;

            return Ok(new PatientProfileResponseDto
            {
                Patient_id = patient.PatientId,
                Patient_name = patient.PatientName,
                Patient_age = patient.PatientAge,
                Patient_Gender = patient.PatientGender,
                Patient_city = patient.PatientCity,
                Patient_email = patient.PatientEmail,
                Patient_married = patient.PatientMarried ?? false,
                Patient_phone = patient.PatientPhone,
               
                // ✅ من جدول PatientSickRecord
                Patient_height = sick?.PationtHight,
                Patient_weight = sick?.PationtWeight,
                Patient_anemia = sick?.PatientAnaemia,
                Patient_hypertension = sick?.PatientHypertension,
                Patient_diabetes = sick?.PatientDiabetes,
                Patient_bloodtype = sick?.PatientBloodtype
            });
        }
    }
}