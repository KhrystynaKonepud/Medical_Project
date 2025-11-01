import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import HomePage from './pages/HomePage';
import AdminDashboard from './pages/AdminDashboard';
import CreateDoctor from './pages/CreateDoctor';
import DoctorsList from './pages/DoctorsList';
import EditDoctor from './pages/EditDoctor';
import DoctorDashboard from './pages/doctor/DoctorDashboard';


// 1. Імпортуємо новий лейаут і сторінки пацієнта
import PatientLayout from './pages/patient/PatientLayout';
import PatientProfile from './pages/patient/PatientProfile';
import PatientDoctorsList from './pages/patient/PatientDoctorsList';
import PatientVaccination from './pages/patient/PatientVaccination';
import PatientLabResults from './pages/patient/PatientLabResults';
import PatientHistory from './pages/patient/PatientHistory';
import PatientPrescriptions from './pages/patient/PatientPrescriptions';


// 2. Створюємо хелпер для збереження .container на публічних сторінках
const PublicLayout = ({ children }) => (
    <div className="container py-4">
        {children}
    </div>
);

function App() {
    return (
        <Router>
            {/* 3. Прибираємо глобальний .container (лейаут пацієнта буде на всю ширину) */}
            <Routes>
                {/* Публічні маршрути з .container */}
                <Route path="/" element={<PublicLayout><HomePage /></PublicLayout>} />
                <Route path="/login" element={<PublicLayout><Login /></PublicLayout>} />
                <Route path="/register" element={<PublicLayout><Register /></PublicLayout>} />

                {/* Адмін-маршрути (залишаємо як є, з контейнером) */}
                <Route path="/admin/dashboard" element={<PublicLayout><AdminDashboard /></PublicLayout>} />
                <Route path="/admin/createdoctor" element={<PublicLayout><CreateDoctor /></PublicLayout>} />
                <Route path="/admin/doctors" element={<PublicLayout><DoctorsList /></PublicLayout>} />
                <Route path="/admin/editdoctor/:id" element={<PublicLayout><EditDoctor /></PublicLayout>} />
                <Route path="/doctor/dashboard" element={< DoctorDashboard />} />


                {/* === НОВІ МАРШРУТИ ПАЦІЄНТА === */}
                {/* 4. Встановлюємо PatientLayout як батьківський маршрут */}
                <Route path="/patient/dashboard" element={<PatientLayout />}>
                    {/* При вході на /patient/dashboard, автоматично перекидаємо на 'profile'.
                      Логіка з Login.js та Register.js 
                      ідеально сюди впишеться.
                    */}
                    <Route index element={<Navigate to="profile" replace />} />

                    {/* 5. Дочірні сторінки, що будуть рендеритись всередині PatientLayout */}
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