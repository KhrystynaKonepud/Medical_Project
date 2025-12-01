namespace Mobile_Medical_Center.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Canceled
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
