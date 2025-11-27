"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const apiClient_1 = require("../helpers/apiClient");
const config_1 = require("../config");
const types_1 = require("../types");
describe('Appointments API Integration Tests', () => {
    describe('GET /api/appointments', () => {
        it('should return list of appointments', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            expect(appointments).toBeDefined();
            expect(Array.isArray(appointments)).toBe(true);
            console.log(`[${config_1.config.dbProvider}] Fetched ${appointments.length} appointments`);
        });
        it('should return appointments with correct structure', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            if (appointments.length > 0) {
                const appointment = appointments[0];
                expect(appointment).toHaveProperty('id');
                expect(appointment).toHaveProperty('date');
                expect(appointment).toHaveProperty('reason');
                expect(appointment).toHaveProperty('status');
                expect(appointment).toHaveProperty('patientId');
                expect(appointment).toHaveProperty('doctorId');
                expect(appointment).toHaveProperty('patient');
                expect(appointment).toHaveProperty('doctor');
                expect(typeof appointment.reason).toBe('string');
                expect(typeof appointment.patientId).toBe('number');
                expect(typeof appointment.doctorId).toBe('number');
                console.log(`[${config_1.config.dbProvider}] Appointment structure validated:`, {
                    id: appointment.id,
                    date: appointment.date,
                    status: appointment.status,
                    patientId: appointment.patientId,
                    doctorId: appointment.doctorId
                });
            }
        });
        it('should have valid appointment status', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            const validStatuses = [
                types_1.AppointmentStatus.Scheduled,
                types_1.AppointmentStatus.Completed,
                types_1.AppointmentStatus.Canceled
            ];
            appointments.forEach(appointment => {
                expect(validStatuses).toContain(appointment.status);
            });
            console.log(`[${config_1.config.dbProvider}] All appointments have valid status values`);
        });
        it('should have valid date format', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            appointments.forEach(appointment => {
                const date = new Date(appointment.date);
                expect(date).toBeInstanceOf(Date);
                expect(isNaN(date.getTime())).toBe(false);
            });
            console.log(`[${config_1.config.dbProvider}] All appointments have valid date format`);
        });
        it('should include patient and doctor relations', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            if (appointments.length > 0) {
                const appointment = appointments[0];
                expect(appointment.patient).toBeDefined();
                expect(appointment.doctor).toBeDefined();
                expect(appointment.patient.id).toBe(appointment.patientId);
                expect(appointment.doctor.id).toBe(appointment.doctorId);
                console.log(`[${config_1.config.dbProvider}] Appointment relations validated:`, {
                    appointmentId: appointment.id,
                    patientId: appointment.patientId,
                    doctorId: appointment.doctorId,
                    patientName: appointment.patient.user?.fullName,
                    doctorName: appointment.doctor.user?.fullName
                });
            }
        });
    });
    describe('GET /api/appointments/:id', () => {
        it('should return a single appointment by id', async () => {
            const allAppointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            if (allAppointments.length > 0) {
                const firstAppointmentId = allAppointments[0].id;
                const appointment = await apiClient_1.apiClient.get(config_1.endpoints.appointmentById(firstAppointmentId));
                expect(appointment).toBeDefined();
                expect(appointment.id).toBe(firstAppointmentId);
                expect(appointment).toHaveProperty('patient');
                expect(appointment).toHaveProperty('doctor');
                console.log(`[${config_1.config.dbProvider}] Fetched appointment with ID ${firstAppointmentId}:`, appointment);
            }
        });
        it('should return 404 for non-existent appointment', async () => {
            const nonExistentId = 999999;
            try {
                await apiClient_1.apiClient.get(config_1.endpoints.appointmentById(nonExistentId));
                fail('Expected 404 error but request succeeded');
            }
            catch (error) {
                expect(error.response?.status).toBe(404);
                console.log(`[${config_1.config.dbProvider}] Correctly returned 404 for appointment ID ${nonExistentId}`);
            }
        });
    });
    describe('Appointment Data Integrity', () => {
        it('should have matching patient and doctor IDs in relations', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            appointments.forEach(appointment => {
                if (appointment.patient) {
                    expect(appointment.patient.id).toBe(appointment.patientId);
                }
                if (appointment.doctor) {
                    expect(appointment.doctor.id).toBe(appointment.doctorId);
                }
            });
            console.log(`[${config_1.config.dbProvider}] All appointment relations have matching IDs`);
        });
        it('should have non-empty reason', async () => {
            const appointments = await apiClient_1.apiClient.get(config_1.endpoints.appointments);
            appointments.forEach(appointment => {
                expect(appointment.reason).toBeTruthy();
                expect(appointment.reason.length).toBeGreaterThan(0);
            });
            console.log(`[${config_1.config.dbProvider}] All appointments have non-empty reasons`);
        });
    });
});
