import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const DoctorEditProfile = () => {
    const navigate = useNavigate();

    // Ініціалізація formData, додано email лише для відображення
    const [formData, setFormData] = useState({
        fullName: '',
        address: '',
        dateOfBirth: '',
        gender: '',
        phoneNumber: '',
        specialization: '',
        experienceYears: '',
        bio: '',
        email: ''
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
                    // Форматуємо дату для <input type="date">
                    dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : '',
                    gender: data.gender || '',
                    phoneNumber: data.phoneNumber || '',
                    specialization: data.specialization || '',
                    // ExperienceYears може бути 0, перетворюємо його на рядок для input
                    experienceYears: data.experienceYears ? String(data.experienceYears) : '',
                    bio: data.bio || '',
                    email: data.email || ''
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

        // Хелпер для перетворення порожнього рядка на null.
        const safeNull = (value) => (value === '' || value === undefined) ? null : value;

        if (!formData.fullName.trim()) {
            setMessage('Поле "ПІБ" є обов\'язковим.');
            return;
        }

        const { email, ...updatePayload } = formData;

        // Підготовка чистих значень
        const cleanedPhoneNumber = updatePayload.phoneNumber
            ? updatePayload.phoneNumber.replace(/[^0-9]/g, '')
            : null;

        const experienceYearsValue = updatePayload.experienceYears
            ? Number(updatePayload.experienceYears)
            : null;

        // Перетворення Gender (enum) у число (0 або 1)
        let genderValue = null;
        if (updatePayload.gender === 'Male') {
            genderValue = 0; // Male - перше значення enum (0)
        } else if (updatePayload.gender === 'Female') {
            genderValue = 1; // Female - друге значення enum (1)
        }

        // Підготовка даних для відправки
        // Використовуємо PascalCase для відповідності C# моделі
        const payload = {
            FullName: safeNull(updatePayload.fullName),
            Address: safeNull(updatePayload.address),
            Specialization: safeNull(updatePayload.specialization),
            Bio: safeNull(updatePayload.bio),

            PhoneNumber: cleanedPhoneNumber,

            // Відправляємо числове значення або null
            Gender: genderValue,
            DateOfBirth: safeNull(updatePayload.dateOfBirth),
            ExperienceYears: experienceYearsValue,
        };

        console.log('Sending payload (PascalCase, Gender as Int):', payload); // Лог для дебагу

        try {
            await axios.post('/api/doctor/complete-profile', payload);
            setMessage('Профіль успішно оновлено!');

            // Перенаправлення після успішного збереження
            setTimeout(() => {
                navigate('/doctor/dashboard/profile');
            }, 1000);
        } catch (error) {
            console.error("Error updating profile", error);
            const errorData = error.response?.data;
            let errorMessage = 'Сталася невідома помилка при оновленні профілю.';

            // Розширений розбір помилок Problem Details від ASP.NET Core
            if (errorData && errorData.errors) {
                let allErrors = [];
                // Перебір усіх полів, що містять помилки валідації
                for (const key in errorData.errors) {
                    if (errorData.errors.hasOwnProperty(key)) {
                        const errorMessages = errorData.errors[key].join('; ');
                        allErrors.push(`Поле "${key}": ${errorMessages}`);
                    }
                }
                errorMessage = allErrors.join(' | ');
            } else if (errorData?.message) {
                errorMessage = errorData.message;
            } else if (error.message) {
                errorMessage = error.message;
            }

            setMessage(`Помилка валідації: ${errorMessage}`);
        }
    };

    if (loading) return <p className="lead">Завантаження...</p>;

    return (
        <div className="container mt-4">
            <h3 className="display-5 mb-4">Редагування профілю лікаря</h3>
            {message && <div className={`alert ${message.includes('Помилка') || message.includes('Error') ? "alert-danger" : "alert-success"}`}>{message}</div>}

            <form onSubmit={handleSubmit}>

                <div className="mb-3">
                    <label className="form-label">ПІБ</label>
                    <input type="text" className="form-control" name="fullName" value={formData.fullName} onChange={handleChange} required />
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
                    <select className="form-select" name="gender" value={formData.gender || ''} onChange={handleChange}>
                        <option value="">Виберіть</option>
                        <option value="Male">Чоловіча</option>
                        <option value="Female">Жіноча</option>
                    </select>
                </div>
                <div className="mb-3">
                    <label className="form-label">Номер телефону</label>
                    {/* Підказка для формату номера телефону */}
                    <input
                        type="text"
                        className="form-control"
                        name="phoneNumber"
                        value={formData.phoneNumber}
                        onChange={handleChange}
                        placeholder="Наприклад: 0951234567"
                        title="Введіть номер телефону без +380, використовуючи лише цифри"
                    />
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
                <button type="button" className="btn btn-outline-secondary ms-2" onClick={() => navigate('/doctor/dashboard/profile')}>Скасувати</button>
            </form>
        </div>
    );
};

export default DoctorEditProfile;