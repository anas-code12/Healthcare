using Healthcare.Dtos.Appointments;

namespace Healthcare.Services
{
    public interface IAppointmentService
    {
        Task<int> AutoCancelExpiredAppointmentsAsync();
        Task<AppointmentResponse> BookAppointmentAsync(int patientId, BookAppointmentRequest request);
        Task<bool> CancelAppointmentAsync(int appointmentId, int patientId);
        Task<bool> CompleteAppointmentAsync(int appointmentId, int doctorId, CompleteAppointmentRequest req);
        Task<List<object>> GetChronicDiseaseStatsAsync();
        Task<object> GetDiseasesAndMedicinesByDoctorAsync(int doctorId);
        Task<List<DoctorAppointmentResponse>> GetDoctorAppointmentsAsync(int doctorId);
        Task<DoctorAvailabilityResponse> GetDoctorAvailabilityAsync(int doctorId, int clinicId);
        Task<List<HealthMinistryResponse>> GetHealthMinistryDataAsync();
        Task<List<AppointmentResponse>> GetPatientAppointmentsAsync(int patientId);
        Task<List<PatientMedicalHistoryResponse>> GetPatientMedicalHistoryAsync(int patientId);
        Task<List<object>> GetPendingDiagnosisAsync();
        Task<bool> RateAppointmentAsync(int appointmentId, int patientId, int rating);
        Task<bool> SaveAiDiagnosisAsync(AiDiagnosisRequest req);
    }
}