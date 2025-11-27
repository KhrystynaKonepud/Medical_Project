using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_center.Data
{
    public static class DataSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Check if data already exists
            if (context.Doctors.Any() || context.Patients.Any())
            {
                return; // Database already seeded
            }

            // Create doctor users
            var doctorUsers = new List<ApplicationUser>();
            var doctorEmails = new[]
            {
                ("dr.smith@med.local", "Dr. John Smith", "Cardiology"),
                ("dr.johnson@med.local", "Dr. Sarah Johnson", "Neurology"),
                ("dr.williams@med.local", "Dr. Michael Williams", "Pediatrics")
            };

            foreach (var (email, fullName, specialization) in doctorEmails)
            {
                var doctorUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = $"+38044{Random.Shared.Next(1000000, 9999999)}",
                    EmailConfirmed = true,
                    Address = "Kyiv, Ukraine",
                    DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(30, 50)),
                    Gender = Gender.Male
                };

                var result = await userManager.CreateAsync(doctorUser, "Doctor#2025!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");
                    doctorUsers.Add(doctorUser);

                    // Create Doctor entity
                    var doctor = new Doctor
                    {
                        UserId = doctorUser.Id,
                        Specialization = specialization,
                        ExperienceYears = Random.Shared.Next(5, 25),
                        Rating = (decimal)(Random.Shared.NextDouble() * 2 + 3), // 3.0 - 5.0
                        Bio = $"Experienced {specialization} specialist with excellent patient care record."
                    };
                    context.Doctors.Add(doctor);
                }
            }
            await context.SaveChangesAsync();

            // Create patient users
            var patientUsers = new List<ApplicationUser>();
            var patientNames = new[]
            {
                ("patient1@example.com", "Alice Brown"),
                ("patient2@example.com", "Bob Wilson"),
                ("patient3@example.com", "Carol Davis"),
                ("patient4@example.com", "David Miller"),
                ("patient5@example.com", "Emma Garcia")
            };

            foreach (var (email, fullName) in patientNames)
            {
                var patientUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = $"+38044{Random.Shared.Next(1000000, 9999999)}",
                    EmailConfirmed = true,
                    Address = "Kyiv, Ukraine",
                    DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(20, 60)),
                    Gender = Random.Shared.Next(0, 2) == 0 ? Gender.Male : Gender.Female
                };

                var result = await userManager.CreateAsync(patientUser, "Patient#2025!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");
                    patientUsers.Add(patientUser);

                    // Create Patient entity
                    var patient = new Patient
                    {
                        UserId = patientUser.Id,
                        EmergencyContact = $"+38044{Random.Shared.Next(1000000, 9999999)}"
                    };
                    context.Patients.Add(patient);
                }
            }
            await context.SaveChangesAsync();

            // Get created doctors and patients from DB
            var doctors = await context.Doctors.ToListAsync();
            var patients = await context.Patients.ToListAsync();

            // Create appointments
            var appointmentStatuses = new[] { AppointmentStatus.Scheduled, AppointmentStatus.Completed, AppointmentStatus.Canceled };
            var appointmentReasons = new[]
            {
                "Annual checkup",
                "Follow-up consultation",
                "Chest pain evaluation",
                "Headache examination",
                "Vaccination",
                "Blood pressure monitoring"
            };

            for (int i = 0; i < 10; i++)
            {
                var appointment = new Appointment
                {
                    PatientId = patients[Random.Shared.Next(patients.Count)].Id,
                    DoctorId = doctors[Random.Shared.Next(doctors.Count)].Id,
                    Date = DateTime.Now.AddDays(Random.Shared.Next(-30, 30)),
                    Reason = appointmentReasons[Random.Shared.Next(appointmentReasons.Length)],
                    Status = appointmentStatuses[Random.Shared.Next(appointmentStatuses.Length)]
                };
                context.Appointments.Add(appointment);
            }

            // Create medical records for some patients
            for (int i = 0; i < 5; i++)
            {
                var record = new MedicalRecord
                {
                    PatientId = patients[Random.Shared.Next(patients.Count)].Id,
                    RecordDate = DateTime.Now.AddDays(-Random.Shared.Next(1, 365)),
                    Title = "Medical Examination",
                    Diagnosis = "Routine checkup - all clear",
                    Treatment = "No treatment required",
                    Notes = "Patient in good health"
                };
                context.MedicalRecords.Add(record);
            }

            await context.SaveChangesAsync();
        }
    }
}
