import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

export default function DoctorProfile() {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Хелпер для відображення статі
    const displayGender = (genderValue) => {
        // Очікуємо рядок ("Male", "Female") або число (0, 1) з API
        if (genderValue === 'Male' || genderValue === 0) return 'Чоловіча';
        if (genderValue === 'Female' || genderValue === 1) return 'Жіноча';
        return 'Не вказано';
    };

    // Хелпер для форматування дати
    const formatDate = (dateString) => {
        if (!dateString) return "Не вказано";
        try {
            // Використовуємо .split('T')[0] для відображення лише дати,
            // оскільки повний рядок ISO може містити небажаний час
            const dateOnly = dateString.split('T')[0];
            const date = new Date(dateOnly);
            // Перевірка на коректність дати після парсингу
            if (isNaN(date)) return dateString;

            return date.toLocaleDateString('uk-UA', { year: 'numeric', month: 'long', day: 'numeric' });
        } catch {
            return dateString;
        }
    };

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                setLoading(true);
                // Запит до ендпоінту для лікаря
                const { data } = await axios.get('/api/doctor/profile');
                setProfile(data);
                setError("");
            } catch (err) {
                console.error("Failed to fetch doctor profile", err);
                setError("Не вдалося завантажити профіль лікаря. Перевірте підключення або наявність даних.");
            } finally {
                setLoading(false);
            }
        };
        fetchProfile();
    }, []);

    const welcomeMessage = profile?.fullName ? profile.fullName : 'Лікар';

    return (
        <div className="container mt-4">
            <h1 className="display-5 mb-4">Мій профіль</h1>
            <p className="lead">Ласкаво просимо, {welcomeMessage}!</p>

            <div className="card shadow-sm mb-4">
                <div className="card-header bg-primary text-white">
                    <h5 className="mb-0">Ваші професійні та особисті дані</h5>
                </div>
                <div className="card-body">
                    {loading && <div className="spinner-border text-primary" role="status"><span className="visually-hidden">Завантаження...</span></div>}
                    {error && <div className="alert alert-warning">{error}</div>}

                    {profile && !loading && (
                        <ul className="list-group list-group-flush">
                            <li className="list-group-item"><strong>Email:</strong> {profile.email || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Повне ім'я:</strong> {profile.fullName || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Стать:</strong> {displayGender(profile.gender)}</li>
                            <li className="list-group-item"><strong>Дата народження:</strong> {formatDate(profile.dateOfBirth)}</li>
                            <li className="list-group-item"><strong>Адреса:</strong> {profile.address || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Контактний телефон:</strong> {profile.phoneNumber || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Спеціалізація:</strong> {profile.specialization || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Досвід:</strong> {profile.experienceYears ? `${profile.experienceYears} років` : "Не вказано"}</li>
                            <li className="list-group-item"><strong>Біографія:</strong> {profile.bio || "Не вказано"}</li>
                        </ul>
                    )}
                </div>
            </div>

            {/* Кнопка для переходу до редагування профілю */}
            <Link to="/doctor/dashboard/editprofile" className="btn btn-primary btn-lg">
                Редагувати профіль
            </Link>
        </div>
    );
}