import React from 'react';
import { NavLink } from 'react-router-dom';
import { useProfile } from './PatientLayout'; // Імпортуємо наш хук

// Стилі (залишаємо як були)
const sidebarStyle = {
    width: '280px',
    backgroundColor: '#ffffff',
    borderRight: '1px solid #dee2e6',
    padding: '1.5rem 1rem'
};

export default function PatientSidebar() {
    // Отримуємо статус профілю з Context (який надає PatientLayout)
    const { isProfileComplete } = useProfile();

    // Функція для NavLink
    const getNavLinkClass = ({ isActive }) => {
        return isActive ? 'nav-link active fw-bold' : 'nav-link text-dark';
    };

    // Нова функція для заблокованих посилань
    const getDisabledLinkClass = ({ isActive }) => {
        const baseClass = getNavLinkClass({ isActive });
        // Якщо профіль не заповнений, робимо посилання неактивним
        return !isProfileComplete ? `${baseClass} disabled` : baseClass;
    };

    return (
        <nav style={sidebarStyle}>
            <div className="nav flex-column nav-pills" role="tablist" aria-orientation="vertical">
                {/* 1. Профіль ЗАВЖДИ активний */}
                <NavLink
                    to="profile"
                    className={getNavLinkClass}
                >
                    Профіль
                </NavLink>

                {/* 2. Решта посилань блокуються, якщо профіль не заповнений */}
                <NavLink
                    to="doctors"
                    className={getDisabledLinkClass} // Використовуємо новий клас
                    onClick={(e) => !isProfileComplete && e.preventDefault()} // Блокуємо клік
                >
                    Лікарі {/* Оновлена назва */}
                </NavLink>
                <NavLink
                    to="vaccination"
                    className={getDisabledLinkClass}
                    onClick={(e) => !isProfileComplete && e.preventDefault()}
                >
                    Вакцинація {/* Оновлена назва */}
                </NavLink>
                <NavLink
                    to="results"
                    className={getDisabledLinkClass}
                    onClick={(e) => !isProfileComplete && e.preventDefault()}
                >
                    Аналізи {/* Оновлена назва */}
                </NavLink>
                <NavLink
                    to="history"
                    className={getDisabledLinkClass}
                    onClick={(e) => !isProfileComplete && e.preventDefault()}
                >
                    Історія записів {/* Назва без змін */}
                </NavLink>
                <NavLink
                    to="prescriptions"
                    className={getDisabledLinkClass}
                    onClick={(e) => !isProfileComplete && e.preventDefault()}
                >
                    Рецепти {/* Оновлена назва */}
                </NavLink>
            </div>
        </nav>
    );
}