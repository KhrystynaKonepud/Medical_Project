namespace Medical_center.Models
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
        public DateTime Date { get; set; }
        public string Reason { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
