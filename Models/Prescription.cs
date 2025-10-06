namespace Medical_center.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public DateTime DateIssued { get; set; }
        public string Medication { get; set; }       
        public string Dosage { get; set; }           // Дозування та спосіб прийому
        public string? Notes { get; set; }           

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
