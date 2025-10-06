namespace Medical_center.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string EmergencyContact { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();

    }
}
