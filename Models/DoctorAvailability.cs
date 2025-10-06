namespace Medical_center.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime AvailableDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public int AppointmentDurationMinutes { get; set; } = 60;

        public bool IsActive { get; set; } = true;
    }
}
