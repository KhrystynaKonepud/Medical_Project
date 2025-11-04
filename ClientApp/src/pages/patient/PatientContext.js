import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

// 1. Створюємо сам Context
const PatientContext = createContext(null);

// 2. Створюємо "Provider" - компонент-обгортку
export const PatientProvider = ({ children }) => {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // Функція для завантаження/оновлення профілю
    const fetchProfile = async () => {
        try {
            setLoading(true);
            const { data } = await axios.get('/api/patient/profile');
            setProfile(data);
            setError('');
        } catch (err) {
            console.error("Failed to fetch profile", err);
            setError("Не вдалося завантажити профіль.");
        } finally {
            setLoading(false);
        }
    };

    // Завантажуємо профіль при першому рендері
    useEffect(() => {
        fetchProfile();
    }, []);

    // Визначаємо, чи профіль повний (це поле ми додали в PatientController)
    const isProfileComplete = profile?.isProfileComplete === true;

    // 3. Надаємо дані всім дочірнім елементам
    const value = {
        profile,
        loading,
        error,
        isProfileComplete,
        fetchProfile // Надаємо функцію, щоб PatientProfile міг оновити дані
    };

    return (
        <PatientContext.Provider value={value}>
            {children}
        </PatientContext.Provider>
    );
};

// 4. Створюємо кастомний хук для зручного доступу до даних
export const usePatient = () => {
    const context = useContext(PatientContext);
    if (context === null) {
        throw new Error('usePatient must be used within a PatientProvider');
    }
    return context;
};