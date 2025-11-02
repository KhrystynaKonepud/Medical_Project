import React from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';

export default function DoctorHeader() {
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
        <nav className="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
            <div className="container-fluid">
                <Link className="navbar-brand" to="/doctor/dashboard">
                    Кабінет лікаря
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