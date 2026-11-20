using BCrypt.Net;
using Healthcare.Api.Dtos;
using Healthcare.Dtos.doctor;
using Healthcare.Dtos.Patient;
using Healthcare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Healthcare.Services;

public class AuthService
{
    private readonly HealthcareSystemContext _db;
    private readonly IConfiguration _config;

    public AuthService(HealthcareSystemContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ====================== DOCTOR ======================

    public async Task<AuthResponse> RegisterDoctorAsync(DoctorRegisterRequest req)
    {
        var email = req.Email.Trim().ToLower();

        var exists = await _db.Doctors.AnyAsync(d => d.DoctorEmail.ToLower() == email);
        if (exists)
            return new AuthResponse { Success = false, Message = "Email already exists" };

        int? specialtyId = null;
        if (!string.IsNullOrWhiteSpace(req.SpecialtyName))
        {
            var specialty = await _db.Specialties
                .FirstOrDefaultAsync(s => s.SpecialtyName.ToLower() == req.SpecialtyName.Trim().ToLower());
            if (specialty == null)
                return new AuthResponse { Success = false, Message = "Specialty not found" };
            specialtyId = specialty.SpecialtyId;
        }

        var hashed = BCrypt.Net.BCrypt.HashPassword(req.Password);

        // ✅ مفيش صورة أو وقت في الـ register — بيضيفهم بعدين
        var doctor = new Doctor
        {
            DoctorName = req.FullName,
            DoctorEmail = email,
            DoctorPassword = hashed,
            DoctorPhon = req.Phone,
            DoctorCity = req.City,
            LicenseNumber = req.LicenseNumber,
            Certificate = req.Certificate,
            NationalNumber = req.NationalNumber,
            SpecialtyId = specialtyId
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(doctor.DoctorId, doctor.DoctorEmail);

        return new AuthResponse
        {
            Success = true,
            Message = "Doctor registered successfully",
            Data = new
            {
                token,
                doctor = new { id = doctor.DoctorId, name = doctor.DoctorName, email = doctor.DoctorEmail }
            }
        };
    }

    public async Task<AuthResponse> LoginDoctorAsync(DoctorLoginRequest req)
    {
        var email = req.Email.Trim().ToLower();
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.DoctorEmail.ToLower() == email);
        if (doctor is null)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        bool ok = false;
        if (!string.IsNullOrEmpty(doctor.DoctorPassword))
        {
            if (doctor.DoctorPassword.StartsWith("$2"))
                ok = BCrypt.Net.BCrypt.Verify(req.Password, doctor.DoctorPassword);
            else
                ok = req.Password == doctor.DoctorPassword;
        }

        if (!ok)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        // ✅ تحقق من الـ verification
        if (!doctor.IsVerified)
            return new AuthResponse { Success = false, Message = "حسابك لسه مش متحقق منه من وزارة الصحة" };

        var token = GenerateJwtToken(doctor.DoctorId, doctor.DoctorEmail);
        return new AuthResponse
        {
            Success = true,
            Message = "Logged in successfully",
            Data = new
            {
                token,
                doctor = new { id = doctor.DoctorId, name = doctor.DoctorName, email = doctor.DoctorEmail }
            }
        };
    }

    private string GenerateJwtToken(int doctorId, string email)
    {
        var keyValue = _config["Jwt:Key"] ?? throw new Exception("JWT Key missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, doctorId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, "Doctor")
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

    // ====================== PATIENT ======================

    public async Task<AuthResponse> RegisterPatientAsync(PatientRegisterRequest req)
    {
        var email = req.patient_email.Trim().ToLower();

        var exists = await _db.Patients
            .AnyAsync(p => p.PatientEmail.ToLower() == email);

        if (exists)
            return new AuthResponse { Success = false, Message = "Email already exists" };

        var hashed = BCrypt.Net.BCrypt.HashPassword(req.patient_password);

        int? age = null;
        if (req.birth_date.HasValue)
        {
            age = DateTime.Today.Year - req.birth_date.Value.Year;
            if (req.birth_date.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Today.AddYears(-age.Value)) 
                age--;
        }

        string? genderChar = null;
        if (!string.IsNullOrWhiteSpace(req.patient_Gender))
        {
            var g = req.patient_Gender.Trim().ToLower();
            genderChar = g == "male" ? "M" :
                         g == "female" ? "F" :
                         g == "m" ? "M" :
                         g == "f" ? "F" : null;
        }

        var patient = new Patient
        {
            PatientName = req.patient_name,
            PatientEmail = email,
            PatientPassword = hashed,
            PatientPhone = req.patient_phone,
            PatientCity = req.Patient_City,
            PatientAge = age,
            PatientGender = genderChar,
            PatientMarried = req.patient_married
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        var token = GenerateJwtTokenPatient(patient.PatientId, patient.PatientEmail);

        return new AuthResponse
        {
            Success = true,
            Message = "Patient registered successfully",
            Data = new
            {
                token,
                patient = new
                {
                    id = patient.PatientId,
                    name = patient.PatientName,
                    email = patient.PatientEmail
                }
            }
        };
    }

    public async Task<AuthResponse> LoginPatientAsync(PatientLoginRequest req)
    {
        var email = req.patient_email.Trim().ToLower();

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.PatientEmail.ToLower() == email);

        if (patient is null)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        bool ok = false;

        if (!string.IsNullOrEmpty(patient.PatientPassword))
        {
            if (patient.PatientPassword.StartsWith("$2"))
                ok = BCrypt.Net.BCrypt.Verify(req.patient_password, patient.PatientPassword);
            else
                ok = req.patient_password == patient.PatientPassword;
        }

        if (!ok)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var token = GenerateJwtTokenPatient(patient.PatientId, patient.PatientEmail);

        return new AuthResponse
        {
            Success = true,
            Message = "Logged in successfully",
            Data = new
            {
                token,
                patient = new
                {
                    id = patient.PatientId,
                    name = patient.PatientName,
                    email = patient.PatientEmail
                }
            }
        };
    }

    private string GenerateJwtTokenPatient(int patientId, string email)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, patientId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, "Patient")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ExpiresMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}