using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace Medical_center.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; } 

        // Навігаційні властивості до ролей
        public Patient PatientProfile { get; set; }
        public Doctor DoctorProfile { get; set; }
    }
}
