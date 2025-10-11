using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_center.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rating { get; set; } 
        public string? Comment { get; set; } 
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
