import React, { useState, useEffect } from 'react';
import axios from 'axios';

const DoctorProfile = () => {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const { data } = await axios.get('/api/doctor/profile');
                setProfile(data);
            } catch (error) {
                console.error("Error fetching profile", error);
            } finally {
                setLoading(false);
            }
        };
        fetchProfile();
    }, []);

    if (loading) return <p>Завантаження профілю...</p>;
    if (!profile) return <p>Не вдалося завантажити профіль.</p>;

    return (
        <div className="card shadow-sm">
            <div className="card-header">
                <h3>Профіль лікаря</h3>
            </div>
            <div className="card-body">
                <p><strong>ПІБ:</strong> {profile.fullName}</p>
                <p><strong>Email:</strong> {profile.email}</p>
                {/* Тут можна додати інші поля, специфічні для лікаря */}
                <button className="btn btn-outline-primary">Редагувати</button>
            </div>
        </div>
    );
};

export default DoctorProfile;