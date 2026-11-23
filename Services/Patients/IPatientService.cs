using Healthcare.Api.Dtos;
using Healthcare.Dtos.Patient;

namespace Healthcare.Services.Patients
{
    public interface IPatientService
    {
        Task<AuthResponse> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileRequest req);
        Task<AuthResponse> UpsertSickRecordAsync(PatientSickRecordUpsertRequest req);
    }
}