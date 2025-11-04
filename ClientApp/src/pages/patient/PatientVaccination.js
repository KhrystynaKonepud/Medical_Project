import React, { useState } from 'react';
import axios from 'axios';

// Це дуже спрощена форма. В реальності тут має бути вибір лікаря/клініки.
// Припускаємо, що DoctorId = 1 (тестовий лікар)
export default function PatientVaccination() {
    const [vaccineName, setVaccineName] = useState('');
    const [date, setDate] = useState('');
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setError('');

        try {
            const payload = {
                doctorId: 1, // ЗАГЛУШКА
                vaccineName: vaccineName,
                dateAdministered: date
            };
            const { data } = await axios.post('/api/patient-data/book-vaccination', payload);
            setMessage(data.message || "Запис створено!");
            setVaccineName('');
            setDate('');
        } catch (err) {
            setError(err.response?.data?.message || "Помилка запису.");
        }
    };

    return (
        <div>
            <h1 className="display-5">Запис на вакцинацію</h1>
            <p>Тут буде форма запису на вакцинацію...</p>

            {message && <div className="alert alert-success">{message}</div>}
            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label className="form-label">Назва вакцини</label>
                    <input
                        type="text"
                        className="form-control"
                        value={vaccineName}
                        onChange={e => setVaccineName(e.target.value)}
                        required
                    />
                </div>
                <div className="mb-3">
                    <label className="form-label">Бажана дата</label>
                    <input
                        type="date"
                        className="form-control"
                        value={date}
                        onChange={e => setDate(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="btn btn-primary">Записатися</button>
            </form>
        </div>
    );
}