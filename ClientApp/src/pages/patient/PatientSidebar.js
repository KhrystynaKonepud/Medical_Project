import React from 'react';
import { NavLink } from 'react-router-dom';

// Стилі для бокової панелі
const sidebarStyle = {
    width: '280px', // Фіксована ширина сайдбару
    backgroundColor: '#ffffff',
    borderRight: '1px solid #dee2e6',
    padding: '1.5rem 1rem'
};

export default function PatientSidebar() {

    // Функція для NavLink, щоб Bootstrap коректно підсвічував активне посилання
    const getNavLinkClass = ({ isActive }) => {
        return isActive ? 'nav-link active fw-bold' : 'nav-link text-dark';
    };

    return (
        <nav style={sidebarStyle}>
            <div className="nav flex-column nav-pills" role="tablist" aria-orientation="vertical">
                <NavLink to="profile" className={getNavLinkClass}>
                    Профіль
                </NavLink>
                <NavLink to="doctors" className={getNavLinkClass}>
                    Перегляд лікарів
                </NavLink>
                <NavLink to="vaccination" className={getNavLinkClass}>
                    Запис на вакцинацію
                </NavLink>
                <NavLink to="results" className={getNavLinkClass}>
                    Перегляд аналізів
                </NavLink>
                <NavLink to="history" className={getNavLinkClass}>
                    Історія записів
                </NavLink>
                <NavLink to="prescriptions" className={getNavLinkClass}>
                    Мої рецепти
                </NavLink>
            </div>
        </nav>
    );
}