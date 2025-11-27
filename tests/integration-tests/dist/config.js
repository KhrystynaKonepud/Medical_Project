"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.endpoints = exports.config = void 0;
exports.config = {
    apiBaseUrl: process.env.API_BASE_URL || 'https://localhost:7041',
    apiTimeout: parseInt(process.env.API_TIMEOUT || '10000', 10),
    dbProvider: process.env.DB_PROVIDER || 'SqlServer',
};
exports.endpoints = {
    // Patients API v1
    patientsV1: '/api/v1/patients',
    patientByIdV1: (id) => `/api/v1/patients/${id}`,
    // Patients API v2
    patientsV2: '/api/v2/patients',
    patientByIdV2: (id) => `/api/v2/patients/${id}`,
    // Doctors API
    doctors: '/api/doctors',
    doctorById: (id) => `/api/doctors/${id}`,
    // Appointments API
    appointments: '/api/appointments',
    appointmentById: (id) => `/api/appointments/${id}`,
    // Medical Records API
    medicalRecords: '/api/medicalrecords',
    medicalRecordById: (id) => `/api/medicalrecords/${id}`,
    // Reviews API
    reviews: '/api/reviews',
    reviewById: (id) => `/api/reviews/${id}`,
};
