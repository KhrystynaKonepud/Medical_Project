import React, { createContext, useState, useEffect, useContext, useCallback } from 'react';
import { Outlet, useLocation, Navigate } from 'react-router-dom';
import axios from 'axios';
import PatientHeader from './PatientHeader';
import PatientSidebar from './PatientSidebar';
import PatientFooter from './PatientFooter';

// 1. Створюємо Context, щоб ділитися даними профілю
const ProfileContext = createContext(null);

// 2. Функція-хелпер, яка вирішує, чи заповнений профіль
// Ми вважаємо профіль "неповним", якщо будь-яке з цих полів відсутнє
const checkProfileComplete = (profile) => {
    return profile &&
        !!profile.fullName &&
        !!profile.address &&
        !!profile.dateOfBirth &&
        !!profile.phoneNumber; // Телефон тепер теж обов'язковий
};

// Стилі (з попередньої відповіді)
const layoutStyle = { display: 'flex', flexDirection: 'column', minHeight: '100vh' };
const mainStyle = { display: 'flex', flex: 1 };
const contentStyle = { flex: 1, padding: '2rem', backgroundColor: '#f8f9fa' };
const loadingOverlayStyle = {
    position: 'fixed', top: 0, left: 0, width: '100%', height: '100%',
    backgroundColor: 'rgba(255,255,255,0.7)',
    display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 9998
};

// 3. Головний компонент Лейауту
export default function PatientLayout() {
    const [profile, setProfile] = useState(null);
    const [isProfileComplete, setIsProfileComplete] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const location = useLocation();

    // 4. Функція для завантаження/перезавантаження профілю
    const fetchProfile = useCallback(async () => {
        try {
            if (!loading) setLoading(true); // Показуємо спіннер, якщо це перезавантаження
            const { data } = await axios.get('/api/patient/profile'); //
            setProfile(data);
            setIsProfileComplete(checkProfileComplete(data));
            setError("");
        } catch (err) {
            console.error("Failed to fetch profile", err);
            setError("Не вдалося завантажити профіль.");
            setIsProfileComplete(false);
        } finally {
            setLoading(false);
        }
    }, [loading]); // Додаємо loading, щоб уникнути зайвих запитів

    // 5. Завантажуємо профіль при першому рендері
    useEffect(() => {
        fetchProfile();
    }, []); // Викликаємо лише один раз при монтуванні

    // 6. Значення, яке ми передамо всім дочірнім компонентам
    const contextValue = {
        profile,
        loading,
        error,
        isProfileComplete,
        reloadProfile: fetchProfile // Даємо можливість профілю оновити себе
    };

    // 7. Вирішуємо, чи блокувати контент
    // Якщо профіль не заповнений І ми НЕ на сторінці профілю -> БЛОКУВАТИ
    const isAccessBlocked = !isProfileComplete && !location.pathname.endsWith('/profile');

    // 8. Поки профіль вантажиться, показуємо спіннер на весь екран
    if (loading) {
        return (
            <div style={loadingOverlayStyle}>
                <div className="spinner-border text-primary" style={{ width: '3rem', height: '3rem' }} role="status">
                    <span className="visually-hidden">Завантаження...</span>
                </div>
            </div>
        );
    }

    // 9. Якщо була помилка завантаження, не рендеримо кабінет
    if (error) {
        return <div className="alert alert-danger m-5">{error}</div>
    }

    return (
        <ProfileContext.Provider value={contextValue}>
            <div style={layoutStyle}>
                <PatientHeader />
                <div style={mainStyle}>
                    <PatientSidebar />
                    <main style={contentStyle}>
                        {/* Якщо доступ заблоковано, примусово рендеримо 
                          компонент-попередження замість <Outlet />
                        */}
                        {isAccessBlocked ? (
                            <BlockedContentMessage />
                        ) : (
                            <Outlet /> // Тут рендеряться Profile, Doctors тощо.
                        )}
                    </main>
                </div>
                <PatientFooter />
            </div>
        </ProfileContext.Provider>
    );
}

// 10. Компонент-повідомлення для заблокованих сторінок
const BlockedContentMessage = () => (
    <div className="alert alert-warning text-center shadow-sm">
        <h4 className="alert-heading">Профіль не заповнено!</h4>
        <p>Будь ласка, заповніть ваш профіль, щоб отримати доступ до цього розділу.</p>
        <p className="mb-0">
            Перейдіть у <a href="/patient/dashboard/profile" className="alert-link fw-bold">Профіль</a>, щоб внести дані.
        </p>
    </div>
);


// 11. Експортуємо "хук", щоб дочірні компоненти могли легко отримати дані
export const useProfile = () => {
    const context = useContext(ProfileContext);
    if (!context) {
        throw new Error("useProfile 'повинен' бути використаний всередині PatientLayout");
    }
    return context;
};