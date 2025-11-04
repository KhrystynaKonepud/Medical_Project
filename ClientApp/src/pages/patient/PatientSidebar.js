import React from 'react';
import { NavLink } from 'react-router-dom';
import { usePatient } from './PatientContext'; // ⬅️ ІМПОРТ

// Стилі для бокової панелі
const sidebarStyle = {
    width: '280px', // Фіксована ширина сайдбару
    backgroundColor: '#ffffff',
    borderRight: '1px solid #dee2e6',
    padding: '1.5rem 1rem'
};

export default function PatientSidebar() {
    // ⬅️ ОТРИМУЄМО ДАНІ З КОНТЕКСТУ
    const { isProfileComplete, loading } = usePatient();

    // Функція для NavLink
    const getNavLinkClass = ({ isActive }, isProtected = false) => {
        const baseClass = 'nav-link';

        // 1. Перевіряємо, чи посилання захищене І чи профіль не завершений
        if (isProtected && !isProfileComplete && !loading) {
            return `${baseClass} text-muted disabled`; // Bootstrap клас для блокування
        }

        // 2. Стандартна логіка
        return isActive ? `${baseClass} active fw-bold` : `${baseClass} text-dark`;
    };

    return (
        <nav style={sidebarStyle}>
            <div className="nav flex-column nav-pills" role="tablist" aria-orientation="vertical">
                {/* Профіль завжди доступний */}
                <NavLink
                    to="profile"
                    className={(props) => getNavLinkClass(props, false)}
                >
                    Профіль
                </NavLink>

                {/* Захищені посилання */}
                <NavLink
                    to="doctors"
                    className={(props) => getNavLinkClass(props, true)}
                >
                    Перегляд лікарів
                </NavLink>
                <NavLink
                    to="vaccination"
                    className={(props) => getNavLinkClass(props, true)}
                >
                    Запис на вакцинацію
                </NavLink>
                <NavLink
                    to="results"
                    className={(props) => getNavLinkClass(props, true)}
                >
                    Перегляд аналізів
                </NavLink>
                <NavLink
                    to="history"
                    className={(props) => getNavLinkClass(props, true)}
                >
                    Історія записів
                </NavLink>
                <NavLink
                    to="prescriptions"
                    className={(props) => getNavLinkClass(props, true)}
                >
                    Мої рецепти
                </NavLink>
            </div>

            {!isProfileComplete && !loading && (
                <div className="alert alert-warning mt-3" style={{ fontSize: '0.9rem' }}>
                    Щоб активувати меню, заповніть ваш профіль.
                </div>
            )}
        </nav>
    );
}