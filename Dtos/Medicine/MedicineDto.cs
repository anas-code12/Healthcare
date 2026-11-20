namespace Healthcare.Dtos.Appointments
{
    public class MedicineDto
    {
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
       
    }
    public class SendMessageRequest
    {
        public string MessageBody { get; set; } = null!;
        public string? MessageType { get; set; } // add_disease / add_medicine / other
        public string? ItemName { get; set; } // اسم المرض أو الدواء
        public int? SpecialtyId { get; set; } // بييجي تلقائي من الدكتور
    }
}