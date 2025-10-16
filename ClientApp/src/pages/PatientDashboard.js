import React from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const PatientDashboard = () => {
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
        <div>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h1 className="display-5">Кабінет пацієнта</h1>
                <button onClick={handleLogout} className="btn btn-outline-danger">Вийти</button>
            </div>
            <p className="lead">Ласкаво просимо!</p>
            {/* Тут буде подальший функціонал */}
        </div>
    );
};

export default PatientDashboard;