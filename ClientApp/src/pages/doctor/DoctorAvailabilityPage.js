import React, { useState, useEffect } from 'react';
import axios from 'axios';

const DoctorAvailabilityPage = () => {
    const [availabilities, setAvailabilities] = useState([]);
    const [formData, setFormData] = useState({
        AvailableDate: '',
        StartTime: '',
        AppointmentDurationMinutes: 60
    });
    const [message, setMessage] = useState('');

    // Завантажуємо слоти лікаря
    const fetchAvailabilities = async () => {
        try {
            const { data } = await axios.get('/api/doctor/availability/all');
            setAvailabilities(data);
        } catch (error) {
            console.error("Error fetching availabilities", error);
        }
    };

    useEffect(() => {
        fetchAvailabilities();
    }, []);

    // Обробка полів форми
    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    // Додавання нового слоту
    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.post('/api/doctor/availability/add', {
                AvailableDate: formData.AvailableDate + "T00:00:00",
                StartTime: formData.StartTime + ":00",
                AppointmentDurationMinutes: formData.AppointmentDurationMinutes
            });
            setMessage('Слот додано!');
            setFormData({ AvailableDate: '', StartTime: '', AppointmentDurationMinutes: 60 });
            fetchAvailabilities();
        } catch (error) {
            console.error("Error adding slot:", error.response?.data || error.message);
            setMessage('Сталася помилка при додаванні слоту.');
        }
    };

    // Деактивація слоту
    const handleDeactivate = async (id) => {
        try {
            await axios.post(`/api/doctor/availability/deactivate/${id}`);
            setMessage('Слот деактивовано.');
            fetchAvailabilities();
        } catch (error) {
            console.error(error);
            setMessage('Сталася помилка при деактивації слоту.');
        }
    };

    return (
        <div className="container mt-4">
            <h3>Доступність лікаря</h3>
            {message && <p className="text-success">{message}</p>}

            <form onSubmit={handleSubmit} className="mb-4">
                <div className="mb-3">
                    <label className="form-label">Дата</label>
                    <input type="date" className="form-control" name="AvailableDate" value={formData.AvailableDate} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Час початку</label>
                    <input type="time" className="form-control" name="StartTime" value={formData.StartTime} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Тривалість (хвилин)</label>
                    <input type="number" className="form-control" name="AppointmentDurationMinutes" value={formData.AppointmentDurationMinutes} onChange={handleChange} />
                </div>
                <button type="submit" className="btn btn-primary">Додати слот</button>
            </form>

            <h5>Активні слоти</h5>
            {availabilities.length === 0 && <p>Слоти відсутні</p>}
            {availabilities.length > 0 && (
                <table className="table">
                    <thead>
                        <tr>
                            <th>Дата</th>
                            <th>Час початку</th>
                            <th>Тривалість (хв)</th>
                            <th>Дія</th>
                        </tr>
                    </thead>
                    <tbody>
                        {availabilities.map(a => (
                            <tr key={a.id}>
                                <td>{new Date(a.availableDate).toLocaleDateString()}</td>
                                <td>{a.startTime}</td>
                                <td>{a.appointmentDurationMinutes}</td>
                                <td>
                                    <button className="btn btn-sm btn-danger" onClick={() => handleDeactivate(a.id)}>Деактивувати</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};

export default DoctorAvailabilityPage;