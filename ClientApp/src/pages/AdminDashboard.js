import React from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const AdminDashboard = () => {
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
                <h1 className="display-5">Панель Адміністратора</h1>
                <button onClick={handleLogout} className="btn btn-outline-danger">
                    Вийти
                </button>
            </div>

            <p className="lead mb-4">Ласкаво просимо!</p>

            <div className="d-flex gap-3">
                <button
                    className="btn btn-primary btn-lg"
                    onClick={() => navigate('/admin/createdoctor')}
                >
                    Додати нового лікаря
                </button>

                <button
                    className="btn btn-primary btn-lg"
                    onClick={() => navigate('/admin/doctors')}
                >
                    Переглянути список лікарів
                </button>
            </div>
        </div>
    );
};

export default AdminDashboard;