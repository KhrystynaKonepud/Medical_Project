import React from 'react';
import { NavLink } from 'react-router-dom';

// Стилі для бокової панелі
const sidebarStyle = {
    width: '280px', // Фіксована ширина сайдбару
    backgroundColor: '#ffffff',
    borderRight: '1px solid #dee2e6',
    padding: '1.5rem 1rem'
};

export default function DoctorSidebar() {
    // Функція для NavLink, щоб Bootstrap коректно підсвічував активне посилання
    const getNavLinkClass = ({ isActive }) => {
        // Шляхи відносні до /doctor/
        return isActive ? 'nav-link active fw-bold' : 'nav-link text-dark';
    };

    return (
        <nav style={sidebarStyle}>
            <div className="nav flex-column nav-pills" role="tablist" aria-orientation="vertical">
                <NavLink to="profile" className={getNavLinkClass}>
                    Мій профіль
                </NavLink>
                <NavLink to="patients" className={getNavLinkClass}>
                    Мої пацієнти
                </NavLink>
                <NavLink to="availability" className={getNavLinkClass}>
                    Графік прийомів
                </NavLink>
                <NavLink to="appointments" className={getNavLinkClass}>
                    Записи на прийом
                </NavLink>
                <NavLink to="messages" className={getNavLinkClass}>
                    Повідомлення
                </NavLink>
            </div>
        </nav>
    );
}