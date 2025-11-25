import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import { usePatient } from './PatientContext'; // ⬅️ ІМПОРТ

export default function PatientProfile() {

    // ⬅️ ОТРИМУЄМО ДАНІ ТА ФУНКЦІЇ З КОНТЕКСТУ
    const { profile, loading, error: contextError, fetchProfile } = usePatient();

    const [isEditing, setIsEditing] = useState(false);
    const [formData, setFormData] = useState({
        fullName: '',
        address: '',
        dateOfBirth: '',
        gender: '',
        emergencyContact: ''
    });

    const [message, setMessage] = useState("");
    // Локальна помилка для форми
    const [formError, setFormError] = useState("");

    // Ефект, що реагує на завантаження профілю з КОНТЕКСТУ
    useEffect(() => {
        if (profile) {
            // Ініціалізуємо форму даними з профілю
            setFormData({
                fullName: profile.fullName || '',
                address: profile.address || '',
                dateOfBirth: profile.dateOfBirth ? profile.dateOfBirth.split('T')[0] : '',
                gender: profile.gender ?? '', // 0 = Male, 1 = Female
                emergencyContact: profile.phoneNumber || ''
            });

            // Якщо профіль не заповнений, автоматично вмикаємо режим редагування
            if (!profile.isProfileComplete) {
                setIsEditing(true);
                setFormError("Будь ласка, заповніть ваш профіль, щоб отримати доступ до всіх функцій.");
            } else {
                setFormError("");
            }
        }
    }, [profile]); // Цей ефект спрацює, коли profile з context завантажиться

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");
        setFormError("");

        try {
            const payload = {
                ...formData,
                gender: formData.gender ? Number(formData.gender) : null,
                dateOfBirth: formData.dateOfBirth || null
            };

            await axios.post('/api/patient/complete-profile', payload);

            setMessage("Профіль успішно оновлено.");
            setIsEditing(false);

            // ⬅️ ВИКЛИКАЄМО ФУНКЦІЮ З КОНТЕКСТУ, щоб оновити глобальний стан
            fetchProfile();

        } catch (err) {
            console.error("Failed to update profile", err);
            setFormError(err.response?.data?.message || "Помилка оновлення профілю.");
        }
    };

    const welcomeMessage = profile?.fullName ? profile.fullName : 'Пацієнт';

    if (loading) {
        return <div className="spinner-border text-primary" role="status"><span className="visually-hidden">Loading...</span></div>;
    }

    // Відображаємо загальну помилку, якщо контекст не зміг завантажитись
    if (contextError) {
        return <div className="alert alert-danger">{contextError}</div>;
    }

    return (
        <div>
            <h1 className="display-5 mb-4">Профіль пацієнта</h1>
            <p className="lead">Ласкаво просимо, {welcomeMessage}!</p>

            {/* Кнопки для демонстрації API Versioning */}
            <div className="card shadow-sm mb-4">
                <div className="card-header">
                    <h5 className="mb-0">API Versioning Demo</h5>
                </div>
                <div className="card-body">
                    <p className="text-muted mb-3">Демонстрація різних версій API:</p>
                    <div className="d-flex gap-2">
                        <Link to="/patients-v1" className="btn btn-info">
                            Пацієнти v1 (базова версія)
                        </Link>
                        <Link to="/patients-v2" className="btn btn-success">
                            Пацієнти v2 (розширена версія)
                        </Link>
                    </div>
                </div>
            </div>

            {/* Локальні помилки та повідомлення форми */}
            {formError && <div className="alert alert-warning">{formError}</div>}
            {message && <div className="alert alert-success">{message}</div>}

            <div className="card shadow-sm">
                <div className="card-header d-flex justify-content-between align-items-center">
                    <h5 className="mb-0">Ваші дані</h5>
                    {!isEditing && profile?.isProfileComplete && (
                        <button className="btn btn-outline-primary btn-sm" onClick={() => setIsEditing(true)}>
                            Редагувати
                        </button>
                    )}
                </div>
                <div className="card-body">
                    {isEditing ? (
                        <form onSubmit={handleSubmit}>
                            {/* ... (поля форми залишаються без змін) ... */}
                            <div className="mb-3">
                                <label className="form-label">Повне ім'я</label>
                                <input type="text" name="fullName" value={formData.fullName} onChange={handleChange} className="form-control" />
                            </div>
                            <div className="mb-3">
                                <label className="form-label">Адреса</label>
                                <input type="text" name="address" value={formData.address} onChange={handleChange} className="form-control" />
                            </div>
                            <div className="mb-3">
                                <label className="form-label">Дата народження</label>
                                <input type="date" name="dateOfBirth" value={formData.dateOfBirth} onChange={handleChange} className="form-control" />
                            </div>
                            <div className="mb-3">
                                <label className="form-label">Стать</label>
                                <select name="gender" value={formData.gender} onChange={handleChange} className="form-select">
                                    <option value="">Не вказано</option>
                                    <option value="0">Чоловіча</option>
                                    <option value="1">Жіноча</option>
                                </select>
                            </div>
                            <div className="mb-3">
                                <label className="form-label">Номер телефону</label>
                                <input type="tel" name="emergencyContact" value={formData.emergencyContact} onChange={handleChange} className="form-control" placeholder="+380XXXXXXXXX" />
                            </div>

                            <button type="submit" className="btn btn-primary">Зберегти</button>
                            {profile?.isProfileComplete && (
                                <button type="button" className="btn btn-light ms-2" onClick={() => { setIsEditing(false); setFormError(""); }}>
                                    Скасувати
                                </button>
                            )}
                        </form>
                    ) : (
                        <ul className="list-group list-group-flush">
                            <li className="list-group-item"><strong>Email:</strong> {profile?.email}</li>
                            <li className="list-group-item"><strong>Повне ім'я:</strong> {profile?.fullName || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Адреса:</strong> {profile?.address || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Дата народження:</strong> {profile?.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : "Не вказано"}</li>
                            <li className="list-group-item"><strong>Стать:</strong> {profile?.gender === 0 ? 'Чоловіча' : profile?.gender === 1 ? 'Жіноча' : 'Не вказано'}</li>
                            <li className="list-group-item"><strong>Телефон:</strong> {profile?.phoneNumber || "Не вказано"}</li>
                        </ul>
                    )}
                </div>
            </div>
        </div>
    );
}