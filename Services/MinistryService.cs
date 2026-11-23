using Healthcare.Api.Dtos;
using Healthcare.Dtos.doctor;
using Healthcare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Healthcare.Services
{
    public class MinistryService : IMinistryService
    {
        private readonly HealthcareSystemContext _db;
        private readonly IConfiguration _config;

        public MinistryService(HealthcareSystemContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ================================================================
        // Login وزارة الصحة
        // ================================================================
        public async Task<AuthResponse> LoginAsync(MinistryLoginRequest req)
        {
            var email = req.Email.Trim().ToLower();
            var ministry = await _db.MinistryOfHealths
                .FirstOrDefaultAsync(m => m.MinistryOfHealthEmail.ToLower() == email);

            if (ministry == null)
                return new AuthResponse { Success = false, Message = "Invalid email or password" };

            bool ok = ministry.MinistryOfHealthPassword.StartsWith("$2")
                ? BCrypt.Net.BCrypt.Verify(req.Password, ministry.MinistryOfHealthPassword)
                : req.Password == ministry.MinistryOfHealthPassword;

            if (!ok)
                return new AuthResponse { Success = false, Message = "Invalid email or password" };

            var token = GenerateToken(ministry.MinistryOfHealthId);
            return new AuthResponse
            {
                Success = true,
                Message = "Logged in successfully",
                Data = new { token }
            };
        }

        // ================================================================
        // جيب كل الرسايل
        // ================================================================
        public async Task<List<MessageResponse>> GetAllMessagesAsync()
        {
            var messages = await _db.DoctorMessages
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.Specialty)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                MessageId = m.MessageId,
                DoctorId = m.DoctorId,
                DoctorName = m.Doctor?.DoctorName,
                MessageBody = m.MessageBody,
                Sender = m.Sender,
                SpecialtyName = m.Doctor?.Specialty?.SpecialtyName,
                CreatedAt = m.CreatedAt.HasValue
                    ? m.CreatedAt.Value.ToString("yyyy-MM-dd") : null
            }).ToList();
        }
        // ================================================================
        // وزارة الصحة تبعت رسالة لدكتور
        // ================================================================
        public async Task<bool> SendMessageToDoctorAsync(MinistryToDocterMessageRequest req)
        {
            var doctor = await _db.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == req.DoctorId);

            if (doctor == null)
                throw new Exception("الدكتور مش موجود");

            var message = new DoctorMessage
            {
                DoctorId = req.DoctorId,
                MessageBody = req.MessageBody,
                Sender = "Ministry",
                CreatedAt = DateTime.Now
            };

            _db.DoctorMessages.Add(message);
            await _db.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // تحقق من الدكتور
        // ================================================================
        public async Task<bool> VerifyDoctorAsync(VerifyDoctorRequest req)
        {
            var doctor = await _db.Doctors
                .FirstOrDefaultAsync(d => d.LicenseNumber == req.LicenseNumber);

            if (doctor == null)
                throw new Exception("الدكتور مش موجود");

            doctor.IsVerified = req.IsVerified;
            await _db.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // إضافة مرض
        // ================================================================
        public async Task<bool> AddDiseaseAsync(AddDiseaseRequest req)
        {
            var exists = await _db.Diseases
                .AnyAsync(d => d.DiseasesName == req.DiseasesName
                            && d.SpecialtyId == req.SpecialtyId);
            if (exists)
                throw new Exception("المرض ده موجود بالفعل في نفس التخصص");

            _db.Diseases.Add(new Disease
            {
                DiseasesName = req.DiseasesName,
                disease_severity = req.DiseaseSeverity,
                SpecialtyId = req.SpecialtyId
            });

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<List<object>> GetDoctorsLicenseNumbersAsync()
        {
            return await _db.Doctors
                .Include(d => d.Specialty)
                .Select(d => new
                {
                    doctorId = d.DoctorId,
                    doctorName = d.DoctorName,
                    licenseNumber = d.LicenseNumber,
                    isVerified = d.IsVerified,
                    specialty = d.Specialty!.SpecialtyName
                })
                .Cast<object>()
                .ToListAsync();
        }

        // ================================================================
        // إضافة دواء
        // ================================================================
        public async Task<bool> AddMedicineAsync(AddMedicineRequest req)
        {
            var exists = await _db.Medicines
                .AnyAsync(m => m.MedicineName == req.MedicineName
                            && m.SpecialtyId == req.SpecialtyId);
            if (exists)
                throw new Exception("الدواء ده موجود بالفعل في نفس التخصص");

            _db.Medicines.Add(new Medicine
            {
                MedicineName = req.MedicineName,
                SpecialtyId = req.SpecialtyId
            });

            await _db.SaveChangesAsync();
            return true;
        }


        // ================================================================
        // JWT Token
        // ================================================================
        private string GenerateToken(int ministryId)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, ministryId.ToString()),
                new Claim(ClaimTypes.Role, "Ministry")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}