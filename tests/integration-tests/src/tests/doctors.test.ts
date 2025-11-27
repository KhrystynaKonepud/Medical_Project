import { apiClient } from '../helpers/apiClient';
import { endpoints, config } from '../config';
import { Doctor } from '../types';

describe('Doctors API Integration Tests', () => {
  describe('GET /api/doctors', () => {
    it('should return list of doctors', async () => {
      const doctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      expect(doctors).toBeDefined();
      expect(Array.isArray(doctors)).toBe(true);

      console.log(`[${config.dbProvider}] Fetched ${doctors.length} doctors`);
    });

    it('should return doctors with correct structure', async () => {
      const doctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      if (doctors.length > 0) {
        const doctor = doctors[0];

        expect(doctor).toHaveProperty('id');
        expect(doctor).toHaveProperty('specialization');
        expect(doctor).toHaveProperty('experienceYears');
        expect(doctor).toHaveProperty('userId');
        expect(doctor).toHaveProperty('user');

        expect(typeof doctor.specialization).toBe('string');
        expect(typeof doctor.experienceYears).toBe('number');

        console.log(`[${config.dbProvider}] Doctor structure validated:`, {
          id: doctor.id,
          specialization: doctor.specialization,
          experienceYears: doctor.experienceYears,
          rating: doctor.rating
        });
      }
    });

    it('should have valid experience years', async () => {
      const doctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      doctors.forEach(doctor => {
        expect(doctor.experienceYears).toBeGreaterThanOrEqual(0);
        expect(doctor.experienceYears).toBeLessThanOrEqual(100);
      });

      console.log(`[${config.dbProvider}] All doctors have valid experience years`);
    });

    it('should have valid rating if present', async () => {
      const doctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      doctors.forEach(doctor => {
        if (doctor.rating !== null && doctor.rating !== undefined) {
          expect(doctor.rating).toBeGreaterThanOrEqual(0);
          expect(doctor.rating).toBeLessThanOrEqual(5);
        }
      });

      console.log(`[${config.dbProvider}] All doctors have valid ratings`);
    });
  });

  describe('GET /api/doctors/:id', () => {
    it('should return a single doctor by id', async () => {
      const allDoctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      if (allDoctors.length > 0) {
        const firstDoctorId = allDoctors[0].id;
        const doctor = await apiClient.get<Doctor>(endpoints.doctorById(firstDoctorId));

        expect(doctor).toBeDefined();
        expect(doctor.id).toBe(firstDoctorId);
        expect(doctor).toHaveProperty('specialization');
        expect(doctor).toHaveProperty('user');

        console.log(`[${config.dbProvider}] Fetched doctor with ID ${firstDoctorId}:`, doctor);
      }
    });

    it('should return 404 for non-existent doctor', async () => {
      const nonExistentId = 999999;

      try {
        await apiClient.get<Doctor>(endpoints.doctorById(nonExistentId));
        fail('Expected 404 error but request succeeded');
      } catch (error: any) {
        expect(error.response?.status).toBe(404);
        console.log(`[${config.dbProvider}] Correctly returned 404 for doctor ID ${nonExistentId}`);
      }
    });
  });

  describe('Doctor User Relations', () => {
    it('should include user information in doctor response', async () => {
      const doctors = await apiClient.get<Doctor[]>(endpoints.doctors);

      if (doctors.length > 0) {
        const doctor = doctors[0];

        expect(doctor.user).toBeDefined();
        expect(doctor.user).toHaveProperty('id');
        expect(doctor.userId).toBe(doctor.user.id);

        console.log(`[${config.dbProvider}] Doctor-User relation validated:`, {
          doctorId: doctor.id,
          userId: doctor.userId,
          userName: doctor.user.fullName || doctor.user.userName
        });
      }
    });
  });
});
