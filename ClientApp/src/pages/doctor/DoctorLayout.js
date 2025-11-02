import React, { useEffect, useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import axios from 'axios';
import DoctorHeader from './DoctorHeader';
import DoctorSidebar from './DoctorSidebar';
import DoctorFooter from './DoctorFooter';

const layoutStyle = {
    display: 'flex',
    flexDirection: 'column',
    minHeight: '100vh'
};

const mainStyle = {
    display: 'flex',
    flex: 1
};

const contentStyle = {
    flex: 1,
    padding: '2rem',
    backgroundColor: '#f8f9fa'
};

export default function DoctorLayout() {
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        // перевірка авторизації лікаря
        axios.get('/api/auth/check', { withCredentials: true })
            .then(response => {
                if (response.data.role !== 'Doctor') {
                    navigate('/login');
                }
            })
            .catch(() => navigate('/login'))
            .finally(() => setLoading(false));
    }, [navigate]);

    if (loading) return <p>Завантаження...</p>;

    return (
        <div style={layoutStyle}>
            <DoctorHeader />
            <div style={mainStyle}>
                <DoctorSidebar />
                <main style={contentStyle}>
                    <Outlet />
                </main>
            </div>
            <DoctorFooter />
        </div>
    );
}