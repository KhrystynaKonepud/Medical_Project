using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_center.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }

        public string? Bio { get; set; }


        // ВАЖЛИВО: задаємо точність для decimal
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Rating { get; set; } = 0m;

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
