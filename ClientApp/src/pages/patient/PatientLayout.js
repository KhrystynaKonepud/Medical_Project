import React from 'react';
import { Outlet } from 'react-router-dom';
import PatientHeader from './PatientHeader';
import PatientSidebar from './PatientSidebar';
import PatientFooter from './PatientFooter';

// Стилі для "приклеєного" футера та макету
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

export default function PatientLayout() {
    return (
        <div style={layoutStyle}>
            <PatientHeader />
            <div style={mainStyle}>
                <PatientSidebar />
                <main style={contentStyle}>
                    {/* Сюди React Router буде "вставляти" сторінки: Profile, Doctors тощо */}
                    <Outlet />
                </main>
            </div>
            <PatientFooter />
        </div>
    );
}