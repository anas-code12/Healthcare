using Healthcare.Api.Dtos;
using Healthcare.Dtos.doctor;
using Healthcare.Dtos.Patient;

namespace Healthcare.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginDoctorAsync(DoctorLoginRequest req);
        Task<AuthResponse> LoginPatientAsync(PatientLoginRequest req);
        Task<AuthResponse> RegisterDoctorAsync(DoctorRegisterRequest req);
        Task<AuthResponse> RegisterPatientAsync(PatientRegisterRequest req);
    }
}