import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const DoctorEditProfile = () => {
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        fullName: '',
        address: '',
        dateOfBirth: '',
        gender: '',
        phoneNumber: '',
        specialization: '',
        experienceYears: '',
        bio: ''
    });
    const [loading, setLoading] = useState(true);
    const [message, setMessage] = useState('');

    // Завантаження профілю лікаря
    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const { data } = await axios.get('/api/doctor/profile');
                setFormData({
                    fullName: data.fullName || '',
                    address: data.address || '',
                    dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : '',
                    gender: data.gender || '',
                    phoneNumber: data.phoneNumber || '',
                    specialization: data.specialization || '',
                    experienceYears: data.experienceYears || '',
                    bio: data.bio || ''
                });
            } catch (error) {
                console.error("Error fetching profile", error);
                setMessage('Не вдалося завантажити профіль.');
            } finally {
                setLoading(false);
            }
        };
        fetchProfile();
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Підготовка даних для відправки
        const payload = {
            ...formData,
            experienceYears: formData.experienceYears ? Number(formData.experienceYears) : null,
            dateOfBirth: formData.dateOfBirth || null
        };

        try {
            await axios.post('/api/doctor/complete-profile', payload);
            setMessage('Профіль успішно оновлено!');
            navigate('/doctor/profile'); // повернення на сторінку профілю
        } catch (error) {
            console.error("Error updating profile", error);
            setMessage('Сталася помилка при оновленні профілю.');
        }
    };

    if (loading) return <p>Завантаження...</p>;

    return (
        <div className="container mt-4">
            <h3>Редагування профілю лікаря</h3>
            {message && <p className={message.includes('помилка') ? "text-danger" : "text-success"}>{message}</p>}
            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label className="form-label">ПІБ</label>
                    <input type="text" className="form-control" name="fullName" value={formData.fullName} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Email</label>
                    <input type="email" className="form-control" name="email" value={formData.email} disabled />
                </div>
                <div className="mb-3">
                    <label className="form-label">Адреса</label>
                    <input type="text" className="form-control" name="address" value={formData.address} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Дата народження</label>
                    <input type="date" className="form-control" name="dateOfBirth" value={formData.dateOfBirth} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Стать</label>
                    <select className="form-select" name="gender" value={formData.gender} onChange={handleChange}>
                        <option value="">Виберіть</option>
                        <option value="Male">Чоловіча</option>
                        <option value="Female">Жіноча</option>
                    </select>
                </div>
                <div className="mb-3">
                    <label className="form-label">Номер телефону</label>
                    <input type="text" className="form-control" name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Спеціалізація</label>
                    <input type="text" className="form-control" name="specialization" value={formData.specialization} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Досвід роботи (років)</label>
                    <input type="number" className="form-control" name="experienceYears" value={formData.experienceYears} onChange={handleChange} min="0" />
                </div>
                <div className="mb-3">
                    <label className="form-label">Біографія</label>
                    <textarea className="form-control" name="bio" value={formData.bio} onChange={handleChange} rows="3"></textarea>
                </div>
                <button type="submit" className="btn btn-primary">Зберегти</button>
            </form>
        </div>
    );
};

export default DoctorEditProfile;