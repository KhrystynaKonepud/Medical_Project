import React, { useState, useEffect } from 'react';
import axios from 'axios';

export default function PatientProfile() {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                setLoading(true);
                // Запит до ендпоінту з PatientController.cs
                const { data } = await axios.get('/api/patient/profile');
                setProfile(data);
                setError("");
            } catch (err) {
                console.error("Failed to fetch profile", err);
                setError("Не вдалося завантажити профіль. Можливо, його ще не доповнено.");
            } finally {
                setLoading(false);
            }
        };
        fetchProfile();
    }, []);

    // Привітання, перенесене з PatientDashboard.js
    const welcomeMessage = profile?.fullName ? profile.fullName : 'Пацієнт';

    return (
        <div>
            <h1 className="display-5 mb-4">Профіль пацієнта</h1>
            <p className="lead">Ласкаво просимо, {welcomeMessage}!</p>

            <div className="card shadow-sm">
                <div className="card-header">
                    <h5 className="mb-0">Ваші дані</h5>
                </div>
                <div className="card-body">
                    {loading && <div className="spinner-border text-primary" role="status"><span className="visually-hidden">Loading...</span></div>}
                    {error && <div className="alert alert-warning">{error}</div>}

                    {profile && !loading && (
                        <ul className="list-group list-group-flush">
                            <li className="list-group-item"><strong>Email:</strong> {profile.email}</li>
                            <li className="list-group-item"><strong>Повне ім'я:</strong> {profile.fullName}</li>
                            <li className="list-group-item"><strong>Адреса:</strong> {profile.address || "Не вказано"}</li>
                            <li className="list-group-item"><strong>Дата народження:</strong> {profile.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : "Не вказано"}</li>
                            <li className="list-group-item"><strong>Стать:</strong> {profile.gender === 0 ? 'Чоловіча' : profile.gender === 1 ? 'Жіноча' : 'Не вказано'}</li>
                        </ul>
                    )}
                </div>
            </div>

            {/* Тут можна додати форму для заповнення профілю (POST на /api/patient/complete-profile) */}
        </div>
    );
}