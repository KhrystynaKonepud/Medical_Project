import React from 'react';
import { Outlet } from 'react-router-dom';
import DoctorHeader from './DoctorHeader';
import DoctorSidebar from './DoctorSidebar';
import DoctorFooter from './DoctorFooter';

// Стилі для футера та макету
const layoutStyle = {
    display: 'flex',
    flexDirection: 'column',
    minHeight: '100vh' // Займає всю висоту екрану
};

const mainStyle = {
    display: 'flex',
    flex: 1 // Займає весь доступний простір між хедером і футером
};

const contentStyle = {
    flex: 1, // Займає весь простір праворуч від сайдбару
    padding: '2rem',
    backgroundColor: '#f8f9fa' // Світлий фон для контенту
};

export default function DoctorLayout() {
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