namespace Medical_center.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }

        public string? Bio { get; set; } 
        public decimal? Rating { get; set; } = 0; // Середній рейтинг лікаря, початково 0 або null

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
    }
}
