// Patient DTOs
export interface PatientDtoV1 {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  emergencyContact: string;
}

export interface PatientStatistics {
  totalAppointments: number;
  totalMedicalRecords: number;
  totalPrescriptions: number;
  totalTestResults: number;
  totalVaccinations: number;
  totalReviews: number;
  lastAppointmentDate: string | null;
}

export interface PatientDtoV2 {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  emergencyContact: string;
  statistics: PatientStatistics;
}

// Doctor
export interface Doctor {
  id: number;
  specialization: string;
  experienceYears: number;
  bio: string | null;
  rating: number | null;
  userId: string;
  user: any;
  appointments?: any[];
}

// Appointment
export enum AppointmentStatus {
  Scheduled = 0,
  Completed = 1,
  Canceled = 2
}

export interface Appointment {
  id: number;
  date: string;
  reason: string;
  status: AppointmentStatus;
  patientId: number;
  patient?: any;
  doctorId: number;
  doctor?: any;
}
