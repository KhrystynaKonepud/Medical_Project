export const config = {
  apiBaseUrl: process.env.API_BASE_URL || 'https://localhost:7041',
  apiTimeout: parseInt(process.env.API_TIMEOUT || '10000', 10),
  dbProvider: process.env.DB_PROVIDER || 'SqlServer',
};

export const endpoints = {
  // Patients API v1
  patientsV1: '/api/v1/patients',
  patientByIdV1: (id: number) => `/api/v1/patients/${id}`,

  // Patients API v2
  patientsV2: '/api/v2/patients',
  patientByIdV2: (id: number) => `/api/v2/patients/${id}`,

  // Doctors API
  doctors: '/api/doctors',
  doctorById: (id: number) => `/api/doctors/${id}`,

  // Appointments API
  appointments: '/api/appointments',
  appointmentById: (id: number) => `/api/appointments/${id}`,

  // Medical Records API
  medicalRecords: '/api/medicalrecords',
  medicalRecordById: (id: number) => `/api/medicalrecords/${id}`,

  // Reviews API
  reviews: '/api/reviews',
  reviewById: (id: number) => `/api/reviews/${id}`,
};
