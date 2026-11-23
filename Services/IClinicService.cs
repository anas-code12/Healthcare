using Healthcare.Dtos.Clinics;

namespace Healthcare.Services
{
    public interface IClinicService
    {
        Task<ClinicResponse> AddBreakAsync(int doctorId, int clinicId, AddBreakRequest request);
        Task<ClinicResponse> AddClinicAsync(int doctorId, ClinicUpsertRequest request);
        Task<bool> DeleteBreakAsync(int doctorId, int clinicId, string breakTime);
        Task<bool> DeleteClinicAsync(int doctorId, int clinicId);
        Task<List<ClinicResponse>> GetDoctorClinicsAsync(int doctorId);
        Task<ClinicResponse> UpdateClinicAsync(int doctorId, int clinicId, ClinicUpsertRequest request);
    }
}