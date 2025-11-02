import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
// === Публічні сторінки ===
import Login from './pages/Login';
import Register from './pages/Register';
import HomePage from './pages/HomePage';

// === Адмін-сторінки ===
import AdminDashboard from './pages/AdminDashboard';
import CreateDoctor from './pages/CreateDoctor';
import DoctorsList from './pages/DoctorsList';
import EditDoctor from './pages/EditDoctor';

// === СТОРІНКИ ЛІКАРЯ ТА НОВИЙ ЛЕЙАУТ ===
import DoctorLayout from './pages/doctor/DoctorLayout';
import DoctorDashboard from './pages/doctor/DoctorDashboard';
import DoctorProfile from './pages/doctor/DoctorProfile';
// Додаємо імпорт DoctorEditProfile
import DoctorEditProfile from './pages/doctor/DoctorEditProfile'; // <-- НОВИЙ ІМПОРТ
import DoctorPatients from './pages/doctor/DoctorPatients';
import DoctorAvailabilityPage from './pages/doctor/DoctorAvailabilityPage';
import DoctorMessages from './pages/doctor/DoctorMessages';

// === СТОРІНКИ ПАЦІЄНТА ===
import PatientLayout from './pages/patient/PatientLayout';
import PatientProfile from './pages/patient/PatientProfile';
import PatientDoctorsList from './pages/patient/PatientDoctorsList';
import PatientVaccination from './pages/patient/PatientVaccination';
import PatientLabResults from './pages/patient/PatientLabResults';
import PatientHistory from './pages/patient/PatientHistory';
import PatientPrescriptions from './pages/patient/PatientPrescriptions';

// Допоміжний лейаут для публічних сторінок
const PublicLayout = ({ children }) => (
    <div className="container py-4">
        {children}
    </div>
);

function App() {
    return (
        <Router>
            <Routes>
                {/* Публічні маршрути з .container */}
                <Route path="/" element={<PublicLayout><HomePage /></PublicLayout>} />
                <Route path="/login" element={<PublicLayout><Login /></PublicLayout>} />
                <Route path="/register" element={<PublicLayout><Register /></PublicLayout>} />

                {/* Адмін-маршрути */}
                <Route path="/admin/dashboard" element={<PublicLayout><AdminDashboard /></PublicLayout>} />
                <Route path="/admin/createdoctor" element={<PublicLayout><CreateDoctor /></PublicLayout>} />
                <Route path="/admin/doctors" element={<PublicLayout><DoctorsList /></PublicLayout>} />
                <Route path="/admin/editdoctor/:id" element={<PublicLayout><EditDoctor /></PublicLayout>} />

                {/* === МАРШРУТИ ЛІКАРЯ (З LAYOUT) === */}
                <Route path="/doctor/dashboard" element={<DoctorLayout />}>
                    <Route index element={<Navigate to="profile" replace />} />

                    {/* Дочірні сторінки, що будуть рендеритись всередині DoctorLayout */}
                    <Route path="profile" element={<DoctorProfile />} />
                    <Route path="editprofile" element={<DoctorEditProfile />} /> {/* <-- ДОДАНО МАРШРУТ РЕДАГУВАННЯ */}
                    <Route path="patients" element={<DoctorPatients />} />
                    <Route path="availability" element={<DoctorAvailabilityPage />} />
                    <Route path="messages" element={<DoctorMessages />} />
                </Route>

                {/* === МАРШРУТИ ПАЦІЄНТА === */}
                <Route path="/patient/dashboard" element={<PatientLayout />}>
                    <Route index element={<Navigate to="profile" replace />} />

                    {/* Дочірні сторінки, що будуть рендеритись всередині PatientLayout */}
                    <Route path="profile" element={<PatientProfile />} />
                    <Route path="doctors" element={<PatientDoctorsList />} />
                    <Route path="vaccination" element={<PatientVaccination />} />
                    <Route path="results" element={<PatientLabResults />} />
                    <Route path="history" element={<PatientHistory />} />
                    <Route path="prescriptions" element={<PatientPrescriptions />} />
                </Route>

                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Router>
    );
}

export default App;