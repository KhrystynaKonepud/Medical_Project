namespace Medical_center.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public DateTime RecordDate { get; set; }

        public string Title { get; set; }       
        public string Diagnosis { get; set; }   
        public string Treatment { get; set; }   
        public string? Notes { get; set; }      

        public int PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}
