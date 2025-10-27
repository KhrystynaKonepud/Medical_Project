import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useProfile } from './PatientLayout'; // Імпортуємо наш хук

// Хелпер для форматування дати в YYYY-MM-DD
const formatDateForInput = (dateString) => {
    if (!dateString) return "";
    try {
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    } catch (e) {
        return "";
    }
};

export default function PatientProfile() {
    // Отримуємо глобальний стан з Context (з PatientLayout)
    const { profile, loading, isProfileComplete, reloadProfile } = useProfile();

    // Локальний стан форми
    const [form, setForm] = useState({
        fullName: "",
        email: "",
        address: "",
        dateOfBirth: "",
        gender: "2", // "2" - Не вказано
        phoneNumber: "",
    });

    const [pending, setPending] = useState(false);
    const [serverError, setServerError] = useState("");
    const [successMessage, setSuccessMessage] = useState("");

    // Коли глобальний профіль завантажується, оновлюємо локальну форму
    useEffect(() => {
        if (profile) {
            setForm({
                fullName: profile.fullName || "",
                email: profile.email || "",
                address: profile.address || "",
                dateOfBirth: formatDateForInput(profile.dateOfBirth),
                gender: profile.gender !== null ? profile.gender.toString() : "2",
                phoneNumber: profile.phoneNumber || "",
            });
        }
    }, [profile]); // Цей ефект спрацює, коли 'profile' з Context завантажиться

    const onChange = (e) => {
        const { name, value } = e.target;
        setForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setPending(true);
        setServerError("");
        setSuccessMessage("");

        const payload = {
            ...form,
            gender: parseInt(form.gender, 10), // Конвертуємо стать у число
        };

        try {
            // Використовуємо POST-ендпоінт
            await axios.post('/api/patient/complete-profile', payload);

            // Після успішного збереження, викликаємо reloadProfile()
            // Це оновить глобальний стан (isProfileComplete) і розблокує сайдбар
            await reloadProfile();

            setSuccessMessage("Профіль успішно оновлено!");
        } catch (err) {
            const msg = err.response?.data?.message ||
                err.response?.data?.errors?.[0]?.description ||
                "Помилка збереження. Перевірте дані.";
            setServerError(msg);
        } finally {
            setPending(false);
        }
    };

    // Спіннер, поки завантажується глобальний профіль
    if (loading) {
        return (
            <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    const welcomeMessage = profile?.fullName || 'Пацієнт'; //

    return (
        <div>
            <h1 className="display-5 mb-3">Профіль пацієнта</h1>
            <p className="lead">Ласкаво просимо, {welcomeMessage}!</p>

            {!isProfileComplete && (
                <div className="alert alert-warning shadow-sm">
                    <strong>Будь ласка, заповніть профіль.</strong> Це необхідно для доступу до інших функцій кабінету.
                </div>
            )}

            <div className="card shadow-sm">
                <div className="card-header">
                    <h5 className="mb-0">Ваші дані</h5>
                </div>
                <div className="card-body">
                    <form onSubmit={handleSubmit} noValidate>

                        <div className="mb-3">
                            <label className="form-label">Email (не можна змінити)</label>
                            <input
                                type="email"
                                name="email"
                                value={form.email}
                                className="form-control"
                                disabled
                                readOnly
                            />
                        </div>

                        <div className="mb-3">
                            <label className="form-label">Повне ім'я</label>
                            <input
                                type="text"
                                name="fullName"
                                value={form.fullName}
                                onChange={onChange}
                                className="form-control"
                                required
                            />
                        </div>

                        <div className="mb-3">
                            <label className="form-label">Ваш номер телефону (формат +380XXXXXXXXX)</label>
                            <input
                                type="tel"
                                name="phoneNumber"
                                value={form.phoneNumber}
                                onChange={onChange}
                                className="form-control"
                                placeholder="+380XXXXXXXXX"
                                pattern="\+?380\d{9}"
                                required
                            />
                        </div>

                        <div className="mb-3">
                            <label className="form-label">Адреса</label>
                            <input
                                type="text"
                                name="address"
                                value={form.address}
                                onChange={onChange}
                                className="form-control"
                                required
                            />
                        </div>

                        <div className="row">
                            <div className="col-md-6 mb-3">
                                <label className="form-label">Дата народження</label>
                                <input
                                    type="date"
                                    name="dateOfBirth"
                                    value={form.dateOfBirth}
                                    onChange={onChange}
                                    className="form-control"
                                    required
                                />
                            </div>
                            <div className="col-md-6 mb-3">
                                <label className="form-label">Стать</label>
                                <select
                                    name="gender"
                                    value={form.gender}
                                    onChange={onChange}
                                    className="form-select"
                                    required
                                >
                                    <option value="0">Чоловіча</option>
                                    <option value="1">Жіноча</option>
                                    <option value="2">Не вказано</option>
                                </select>
                            </div>
                        </div>

                        {serverError && <div className="alert alert-danger">{serverError}</div>}
                        {successMessage && <div className="alert alert-success">{successMessage}</div>}

                        <button type="submit" className="btn btn-primary" disabled={pending}>
                            {pending ? "Збереження..." : "Зберегти зміни"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}