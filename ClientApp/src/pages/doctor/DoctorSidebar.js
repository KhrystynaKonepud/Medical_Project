import React from 'react';
import { NavLink } from 'react-router-dom';

const sidebarStyle = {
    width: '280px',
    backgroundColor: '#ffffff',
    borderRight: '1px solid #dee2e6',
    padding: '1.5rem 1rem'
};

export default function DoctorSidebar() {
    const getNavLinkClass = ({ isActive }) => {
        return isActive ? 'nav-link active fw-bold' : 'nav-link text-dark';
    };

    return (
        <nav style={sidebarStyle}>
            <div className="nav flex-column nav-pills" role="tablist" aria-orientation="vertical">
                <NavLink to="dashboard" className={getNavLinkClass}>
                    Головна
                </NavLink>
                <NavLink to="profile" className={getNavLinkClass}>
                    Мій профіль
                </NavLink>
                <NavLink to="patients" className={getNavLinkClass}>
                    Пацієнти
                </NavLink>
                <NavLink to="availability" className={getNavLinkClass}>
                    Графік прийомів
                </NavLink>
                <NavLink to="messages" className={getNavLinkClass}>
                    Повідомлення
                </NavLink>
            </div>
        </nav>
    );
}