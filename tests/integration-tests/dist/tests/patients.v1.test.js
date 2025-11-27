"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const apiClient_1 = require("../helpers/apiClient");
const config_1 = require("../config");
describe('Patients API V1 Integration Tests', () => {
    describe('GET /api/v1/patients', () => {
        it('should return list of patients', async () => {
            const patients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV1);
            expect(patients).toBeDefined();
            expect(Array.isArray(patients)).toBe(true);
            console.log(`[${config_1.config.dbProvider}] Fetched ${patients.length} patients from V1 API`);
        });
        it('should return patients with correct V1 structure', async () => {
            const patients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV1);
            if (patients.length > 0) {
                const patient = patients[0];
                expect(patient).toHaveProperty('id');
                expect(patient).toHaveProperty('fullName');
                expect(patient).toHaveProperty('email');
                expect(patient).toHaveProperty('phoneNumber');
                expect(patient).toHaveProperty('emergencyContact');
                // V1 should NOT have statistics
                expect(patient).not.toHaveProperty('statistics');
                console.log(`[${config_1.config.dbProvider}] V1 Patient structure validated:`, patient);
            }
        });
    });
    describe('GET /api/v1/patients/:id', () => {
        it('should return a single patient by id', async () => {
            const allPatients = await apiClient_1.apiClient.get(config_1.endpoints.patientsV1);
            if (allPatients.length > 0) {
                const firstPatientId = allPatients[0].id;
                const patient = await apiClient_1.apiClient.get(config_1.endpoints.patientByIdV1(firstPatientId));
                expect(patient).toBeDefined();
                expect(patient.id).toBe(firstPatientId);
                expect(patient).toHaveProperty('fullName');
                expect(patient).toHaveProperty('email');
                console.log(`[${config_1.config.dbProvider}] Fetched patient with ID ${firstPatientId}:`, patient);
            }
        });
        it('should return 404 for non-existent patient', async () => {
            const nonExistentId = 999999;
            try {
                await apiClient_1.apiClient.get(config_1.endpoints.patientByIdV1(nonExistentId));
                fail('Expected 404 error but request succeeded');
            }
            catch (error) {
                expect(error.response?.status).toBe(404);
                console.log(`[${config_1.config.dbProvider}] Correctly returned 404 for patient ID ${nonExistentId}`);
            }
        });
    });
});
