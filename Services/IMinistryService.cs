using Healthcare.Api.Dtos;
using Healthcare.Dtos.doctor;

namespace Healthcare.Services
{
    public interface IMinistryService
    {
        Task<bool> AddDiseaseAsync(AddDiseaseRequest req);
        Task<bool> AddMedicineAsync(AddMedicineRequest req);
        Task<List<MessageResponse>> GetAllMessagesAsync();
        Task<List<object>> GetDoctorsLicenseNumbersAsync();
        Task<AuthResponse> LoginAsync(MinistryLoginRequest req);
        Task<bool> SendMessageToDoctorAsync(MinistryToDocterMessageRequest req);
        Task<bool> VerifyDoctorAsync(VerifyDoctorRequest req);
    }
}