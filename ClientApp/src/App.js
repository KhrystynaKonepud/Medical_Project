import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import HomePage from './pages/HomePage';
import PatientDashboard from './pages/PatientDashboard';
import AdminDashboard from './pages/AdminDashboard';
import CreateDoctor from './pages/CreateDoctor';
import DoctorsList from './pages/DoctorsList';
import EditDoctor from './pages/EditDoctor';

function App() {
    return (
        <Router>
            <div className="container py-4">
                <Routes>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/patient/dashboard" element={<PatientDashboard />} />
                    <Route path="/admin/dashboard" element={<AdminDashboard />} />
                    <Route path="/admin/createdoctor" element={<CreateDoctor />} />
                    <Route path="/admin/doctors" element={<DoctorsList />} />
                    <Route path="/admin/editdoctor/:id" element={<EditDoctor />} />
                    <Route path="*" element={<Navigate to="/" />} />
                </Routes>
            </div>
        </Router>
    );
}

export default App;