"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const apiClient_1 = require("../helpers/apiClient");
const config_1 = require("../config");
describe('Patients API V2 Integration Tests', () => {
    describe('GET /api/v2/patients', () => {
        it('should return list of patients with statistics', async () => {
            const patients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV2);
            expect(patients).toBeDefined();
            expect(Array.isArray(patients)).toBe(true);
            console.log(`[${config_1.config.dbProvider}] Fetched ${patients.length} patients from V2 API`);
        });
        it('should return patients with correct V2 structure including statistics', async () => {
            const patients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV2);
            if (patients.length > 0) {
                const patient = patients[0];
                // Basic fields from V1
                expect(patient).toHaveProperty('id');
                expect(patient).toHaveProperty('fullName');
                expect(patient).toHaveProperty('email');
                expect(patient).toHaveProperty('phoneNumber');
                expect(patient).toHaveProperty('emergencyContact');
                // V2 specific - statistics
                expect(patient).toHaveProperty('statistics');
                expect(patient.statistics).toHaveProperty('totalAppointments');
                expect(patient.statistics).toHaveProperty('totalMedicalRecords');
                expect(patient.statistics).toHaveProperty('totalPrescriptions');
                expect(patient.statistics).toHaveProperty('totalTestResults');
                expect(patient.statistics).toHaveProperty('totalVaccinations');
                expect(patient.statistics).toHaveProperty('totalReviews');
                expect(patient.statistics).toHaveProperty('lastAppointmentDate');
                console.log(`[${config_1.config.dbProvider}] V2 Patient with statistics:`, patient);
            }
        });
        it('should have statistics with correct data types', async () => {
            const patients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV2);
            if (patients.length > 0) {
                const patient = patients[0];
                const stats = patient.statistics;
                expect(typeof stats.totalAppointments).toBe('number');
                expect(typeof stats.totalMedicalRecords).toBe('number');
                expect(typeof stats.totalPrescriptions).toBe('number');
                expect(typeof stats.totalTestResults).toBe('number');
                expect(typeof stats.totalVaccinations).toBe('number');
                expect(typeof stats.totalReviews).toBe('number');
                // lastAppointmentDate can be null or string
                if (stats.lastAppointmentDate !== null) {
                    expect(typeof stats.lastAppointmentDate).toBe('string');
                }
                console.log(`[${config_1.config.dbProvider}] V2 Statistics validated:`, stats);
            }
        });
    });
    describe('GET /api/v2/patients/:id', () => {
        it('should return a single patient by id with statistics', async () => {
            const allPatients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV2);
            if (allPatients.length > 0) {
                const firstPatientId = allPatients[0].id;
                const patient = await apiClient_1.apiClient.get(config_1.endpoints.patientByIdV2(firstPatientId));
                expect(patient).toBeDefined();
                expect(patient.id).toBe(firstPatientId);
                expect(patient).toHaveProperty('statistics');
                console.log(`[${config_1.config.dbProvider}] Fetched patient V2 with ID ${firstPatientId}:`, patient);
            }
        });
        it('should return 404 for non-existent patient', async () => {
            const nonExistentId = 999999;
            try {
                await apiClient_1.apiClient.get(config_1.endpoints.patientByIdV2(nonExistentId));
                fail('Expected 404 error but request succeeded');
            }
            catch (error) {
                expect(error.response?.status).toBe(404);
                console.log(`[${config_1.config.dbProvider}] Correctly returned 404 for patient ID ${nonExistentId}`);
            }
        });
    });
    describe('V1 vs V2 Compatibility', () => {
        it('should have V2 as superset of V1 (all V1 fields present in V2)', async () => {
            const patientsV1 = await apiClient_1.apiClient.get(config_1.endpoints.patientsV1);
            const patientsV2 = await apiClient_1.apiClient.get(config_1.endpoints.patientsV2);
            if (patientsV1.length > 0 && patientsV2.length > 0) {
                const v1Fields = Object.keys(patientsV1[0]);
                const v2Fields = Object.keys(patientsV2[0]);
                // All V1 fields should exist in V2
                v1Fields.forEach(field => {
                    expect(v2Fields).toContain(field);
                });
                // V2 should have additional 'statistics' field
                expect(v2Fields).toContain('statistics');
                console.log(`[${config_1.config.dbProvider}] V1 fields:`, v1Fields);
                console.log(`[${config_1.config.dbProvider}] V2 fields:`, v2Fields);
            }
        });
    });
});
