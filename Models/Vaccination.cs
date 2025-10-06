namespace Medical_center.Models
{
    public class Vaccination
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public string VaccineName { get; set; }        
        public DateTime DateAdministered { get; set; } 
        public int DoseNumber { get; set; } = 1;       
        public string? Notes { get; set; }             
    }
}
