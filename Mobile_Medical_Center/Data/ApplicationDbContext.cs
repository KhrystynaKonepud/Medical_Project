using Microsoft.EntityFrameworkCore;
using Mobile_Medical_Center.Models;

namespace Mobile_Medical_Center.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public string DbPath { get; }

        public ApplicationDbContext()
        {
            var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DbPath = Path.Combine(dataDir, "medical_center.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Doctor configuration
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient configuration
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment configuration
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better query performance
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.DoctorId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.PatientId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDate);
        }
    }
}