using Healthcare.Dtos.doctor;
using Healthcare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorProfileController : ControllerBase
    {
        private readonly HealthcareSystemContext _context;
        private readonly IWebHostEnvironment _env;

        public DoctorProfileController(HealthcareSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET api/DoctorProfile/all
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Specialty)
                .Include(d => d.Clinics)
                .ToListAsync();

            var result = doctors.SelectMany(d =>
            {
                if (!d.Clinics.Any())
                {
                    return new[]
                    {
                        new
                        {
                            DoctorId = d.DoctorId,
                            ClinicId = (int?)null,
                            Name = d.DoctorName,
                            Specialty = d.Specialty?.SpecialtyName,
                            Location = (string?)null,
                            ClinicName = (string?)null,
                            AverageRating = d.AverageRating,
                            DoctorPrice = (decimal?)null
                        }
                    };
                }

                return d.Clinics.Select(c => new
                {
                    DoctorId = d.DoctorId,
                    ClinicId = (int?)c.ClinicId,
                    Name = d.DoctorName,
                    Specialty = d.Specialty?.SpecialtyName,
                    Location = c.Location,
                    ClinicName = c.ClinicName,
                    AverageRating = d.AverageRating,
                    DoctorPrice = c.DoctorPrice
                });
            }).ToList();

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET api/DoctorProfile/{doctorId}
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetProfile(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.DoctorId == doctorId);

            string? imageUrl = !string.IsNullOrWhiteSpace(doctor.DoctorImge)
                ? $"/api/DoctorProfile/{doctor.DoctorId}/image"
                : null;

            return Ok(new DoctorProfileResponseDto
            {
                DoctorId = doctor.DoctorId,
                DoctorName = doctor.DoctorName,
                DoctorCity = doctor.DoctorCity,
                DoctorPhone = doctor.DoctorPhon,
                DoctorEmail = doctor.DoctorEmail,
                DoctorPrice = clinic?.DoctorPrice,
                Certificate = doctor.Certificate,
                LicenseNumber = doctor.LicenseNumber,
                DoctorImageUrl = imageUrl,
                NationalNumber = doctor.NationalNumber,
                AverageRating = doctor.AverageRating,
                SpecialtyId = doctor.SpecialtyId,
                SpecialtyName = doctor.Specialty?.SpecialtyName,
                ClinicId = clinic?.ClinicId,
                ClinicName = clinic?.ClinicName,
                ClinicPhone = clinic?.ClinicPhone,
                ClinicEmail = clinic?.ClinicEmail,
                ClinicLocation = clinic?.Location
            });
        }

        // ─────────────────────────────────────────────────────────────────
        //  POST api/DoctorProfile/{doctorId}/image
        // ─────────────────────────────────────────────────────────────────
        [HttpPost("{doctorId}/image")]
        public async Task<IActionResult> UploadDoctorImage(int doctorId, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image provided." });

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "doctors");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrWhiteSpace(doctor.DoctorImge))
            {
                var oldPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, doctor.DoctorImge.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{doctorId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            doctor.DoctorImge = $"/uploads/doctors/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Image uploaded successfully.",
                imageUrl = $"/api/DoctorProfile/{doctorId}/image"
            });
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET api/DoctorProfile/{doctorId}/image
        // ─────────────────────────────────────────────────────────────────
        [HttpGet("{doctorId}/image")]
        public async Task<IActionResult> GetDoctorImage(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);

            if (doctor == null || string.IsNullOrWhiteSpace(doctor.DoctorImge))
                return NotFound(new { message = "Image not found." });

            var imagePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, doctor.DoctorImge.TrimStart('/'));

            if (!System.IO.File.Exists(imagePath))
                return NotFound(new { message = "Image file not found." });

            var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
            return File(imageBytes, "image/jpeg");
        }

        // ─────────────────────────────────────────────────────────────────
        //  PUT api/DoctorProfile/{doctorId}
        // ─────────────────────────────────────────────────────────────────
        [HttpPut("{doctorId}")]
        public async Task<IActionResult> UpdateProfile(int doctorId, [FromBody] DoctorProfileUpdateDto dto)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    return BadRequest(new { message = "Current password is required to change password." });

                // ✅ بيتعامل مع الـ plain text والـ BCrypt
                bool passwordCorrect = false;
                if (doctor.DoctorPassword.StartsWith("$2"))
                    passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, doctor.DoctorPassword);
                else
                    passwordCorrect = dto.CurrentPassword == doctor.DoctorPassword;

                if (!passwordCorrect)
                    return BadRequest(new { message = "Current password is incorrect." });

                if (dto.NewPassword.Length < 6)
                    return BadRequest(new { message = "New password must be at least 6 characters." });

                doctor.DoctorPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            if (!string.IsNullOrWhiteSpace(dto.DoctorName)) doctor.DoctorName = dto.DoctorName;
            if (!string.IsNullOrWhiteSpace(dto.DoctorPhone)) doctor.DoctorPhon = dto.DoctorPhone;
            if (!string.IsNullOrWhiteSpace(dto.DoctorCity)) doctor.DoctorCity = dto.DoctorCity;

            if (!string.IsNullOrWhiteSpace(dto.DoctorEmail))
            {
                bool emailExists = await _context.Doctors
                    .AnyAsync(d => d.DoctorEmail == dto.DoctorEmail && d.DoctorId != doctorId);
                if (emailExists)
                    return BadRequest(new { message = "This email is already used by another doctor." });
                doctor.DoctorEmail = dto.DoctorEmail;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }
    }
}