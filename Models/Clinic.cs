using Medical_center.Attributes;

namespace Medical_center.Models
{
    [GenerateApi]
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }

}
