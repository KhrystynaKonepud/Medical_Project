import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';

const DoctorDashboard = () => {
    const navigate = useNavigate();

    const handleLogout = async () => {
        try {
            await axios.post('/api/auth/logout');
            navigate('/login');
        } catch (error) {
            console.error("Logout failed", error);
            navigate('/login');
        }
    };

    return (
        <div className="container mt-4">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h1 className="display-5">Кабінет лікаря</h1>
                <button onClick={handleLogout} className="btn btn-outline-danger">
                    Вийти
                </button>
            </div>

            <p className="lead mb-4">Ласкаво просимо до вашого кабінету, лікарю!</p>

            {/* Меню посилань */}
            <div className="list-group">
                <Link to="/doctor/profile" className="list-group-item list-group-item-action">
                    Мій профіль
                </Link>
                <Link to="/doctor/patients" className="list-group-item list-group-item-action">
                    Пацієнти
                </Link>
                <Link to="/doctor/availability" className="list-group-item list-group-item-action">
                    Графік прийомів
                </Link>
                <Link to="/doctor/messages" className="list-group-item list-group-item-action">
                    Повідомлення
                </Link>
            </div>
        </div>
    );
};

export default DoctorDashboard;