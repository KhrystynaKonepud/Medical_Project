using Medical_center.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Встановлюємо "Restrict" для Appointment - Doctor/Patient
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescriptions
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pa => pa.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // TestResult
            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Patient)
                .WithMany(p => p.TestResults)
                .HasForeignKey(tr => tr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Doctor)
                .WithMany(d => d.TestResults)
                .HasForeignKey(tr => tr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vaccination
            modelBuilder.Entity<Vaccination>()
                .HasOne(v => v.Patient)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vaccination>()
                .HasOne(v => v.Doctor)
                .WithMany(d => d.Vaccinations)
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
