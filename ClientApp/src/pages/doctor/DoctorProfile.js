import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const DoctorProfile = () => {
    const navigate = useNavigate();
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const { data } = await axios.get('/api/doctor/profile', {
                    withCredentials: true // якщо використовується авторизація
                });
                setProfile(data);
            } catch (err) {
                console.error("Error fetching profile", err);
                setError("Не вдалося отримати дані профілю.");
            } finally {
                setLoading(false);
            }
        };

        fetchProfile();
    }, []);

    if (loading) return <p>Завантаження профілю...</p>;
    if (error) return <p className="text-danger">{error}</p>;
    if (!profile) return <p>Профіль відсутній.</p>;

    return (
        <div className="container mt-4">
            <div className="card shadow-sm">
                <div className="card-header bg-primary text-white">
                    <h3 className="mb-0">Профіль лікаря</h3>
                </div>
                <div className="card-body">
                    <p><strong>ПІБ:</strong> {profile.fullName}</p>
                    <p><strong>Email:</strong> {profile.email}</p>
                    <p><strong>Адреса:</strong> {profile.address}</p>
                    <p><strong>Дата народження:</strong> {profile.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : '—'}</p>
                    <p><strong>Стать:</strong> {profile.gender === 'Male' ? 'Чоловіча' : profile.gender === 'Female' ? 'Жіноча' : '—'}</p>

                    {profile.specialization && (
                        <p><strong>Спеціалізація:</strong> {profile.specialization}</p>
                    )}
                    <p><strong>Досвід роботи:</strong> {profile.experienceYears ?? 0} {profile.experienceYears === 1 ? 'рік' : 'років'}</p>
                    {profile.bio && (
                        <p><strong>Про лікаря:</strong> {profile.bio}</p>
                    )}

                    <button
                        className="btn btn-outline-primary mt-3"
                        onClick={() => navigate('/doctor/editprofile')}
                    >
                        Редагувати профіль
                    </button>
                </div>
            </div>
        </div>
    );
};

export default DoctorProfile;