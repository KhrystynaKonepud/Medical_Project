namespace Medical_center.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public DateTime DateConducted { get; set; }
        public string TestName { get; set; }         
        public string Result { get; set; }           
        public string? Notes { get; set; }           

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
