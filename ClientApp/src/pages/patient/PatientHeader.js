import React from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';

export default function PatientHeader() {
    const navigate = useNavigate();

    // Логіка виходу, перенесена з PatientDashboard.js
    const handleLogout = async () => {
        try {
            // Викликаємо /api/auth/logout, як визначено в AuthController.cs
            await axios.post('/api/auth/logout');
            navigate('/login');
        } catch (error) {
            console.error("Logout failed", error);
            navigate('/login'); // Примусово перекидаємо на логін
        }
    };

    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
            <div className="container-fluid">
                {/* Лого/назва, веде на головну сторінку кабінету */}
                <Link className="navbar-brand" to="/patient/dashboard">
                    Мій Медкабінет
                </Link>
                <button
                    onClick={handleLogout}
                    className="btn btn-outline-light"
                >
                    Вийти
                </button>
            </div>
        </nav>
    );
}