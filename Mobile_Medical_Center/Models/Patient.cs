using System.Text.Json.Serialization;

namespace Mobile_Medical_Center.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "";
        public string Address { get; set; } = "";
        public string EmergencyContact { get; set; } = "";

        // Ignored fields - not sent/received from API
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
