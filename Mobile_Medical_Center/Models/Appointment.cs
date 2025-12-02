namespace Mobile_Medical_Center.Models
{
    public enum AppointmentStatus
    {
        Scheduled, // Заплановано
        Completed, // Проведено
        Canceled   // Скасовано
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        // Navigation properties
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
