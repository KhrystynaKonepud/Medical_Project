import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { usePatient } from './PatientContext'; // ⬅️ Імпорт
import { Link } from 'react-router-dom';     // ⬅️ Імпорт

// Модальне вікно для запису (можна винести в окремий компонент)
const BookingModal = ({ doctor, slot, onClose, onBookingSuccess }) => {
    const [reason, setReason] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async () => {
        if (!reason) {
            setError('Будь ласка, вкажіть причину візиту.');
            return;
        }
        setError('');

        try {
            const payload = {
                doctorId: doctor.id,
                date: slot.availableDate, // YYYY-MM-DD
                startTime: slot.startTime, // HH:MM:SS
                reason: reason
            };
            await axios.post('/api/patient-data/book-appointment', payload);
            onBookingSuccess(doctor.fullName);
            onClose();
        } catch (err) {
            setError(err.response?.data?.message || "Помилка бронювання.");
        }
    };

    return (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title">Запис до лікаря {doctor.fullName}</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body">
                        <p><strong>Дата:</strong> {new Date(slot.availableDate).toLocaleDateString()}</p>
                        <p><strong>Час:</strong> {slot.startTime.substring(0, 5)}</p>
                        <div className="mb-3">
                            <label className="form-label">Причина візиту</label>
                            <textarea
                                className="form-control"
                                rows="3"
                                value={reason}
                                onChange={(e) => setReason(e.target.value)}
                            ></textarea>
                        </div>
                        {error && <div className="alert alert-danger">{error}</div>}
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-light" onClick={onClose}>Скасувати</button>
                        <button type="button" className="btn btn-primary" onClick={handleSubmit}>Підтвердити запис</button>
                    </div>
                </div>
            </div>
        </div>
    );
};


// Компонент зі списком лікарів
export default function PatientDoctorsList() {

    // 1. Отримуємо дані з контексту (перейменовуємо loading, щоб уникнути конфлікту)
    const { isProfileComplete, loading: profileLoading } = usePatient();

    // 2. Локальний стан для цього компонента
    const [doctors, setDoctors] = useState([]);
    const [doctorsLoading, setDoctorsLoading] = useState(true); // Локальний loading
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const [selectedDoctorId, setSelectedDoctorId] = useState(null);
    const [availability, setAvailability] = useState([]);
    const [loadingSlots, setLoadingSlots] = useState(false);

    const [modalSlot, setModalSlot] = useState(null);

    // 3. Завантажуємо лікарів ТІЛЬКИ ЯКЩО профіль заповнений
    useEffect(() => {
        // Не робимо нічого, доки не завантажиться профіль
        if (profileLoading) return;

        // Не завантажуємо лікарів, якщо профіль не повний
        if (!isProfileComplete) {
            setDoctorsLoading(false);
            return;
        }

        // Профіль завантажений і повний, завантажуємо лікарів
        const fetchDoctors = async () => {
            try {
                setDoctorsLoading(true);
                const { data } = await axios.get('/api/patient-data/doctors');
                setDoctors(data);
            } catch (err) {
                setError("Не вдалося завантажити список лікарів.");
            } finally {
                setDoctorsLoading(false);
            }
        };
        fetchDoctors();
    }, [isProfileComplete, profileLoading]); // Залежність від контексту

    // Обробник для показу/приховування розкладу
    const handleShowAvailability = async (doctorId) => {
        if (selectedDoctorId === doctorId) {
            setSelectedDoctorId(null); // Ховаємо, якщо натиснули ще раз
            setAvailability([]);
            return;
        }

        setSelectedDoctorId(doctorId);
        setLoadingSlots(true);
        try {
            const { data } = await axios.get(`/api/patient-data/doctor-availability/${doctorId}`);
            setAvailability(data);
        } catch (err) {
            setError("Помилка завантаження слотів.");
        } finally {
            setLoadingSlots(false);
        }
    };

    // Обробник успішного бронювання (з модалки)
    const handleBookingSuccess = (doctorName) => {
        setSuccess(`Ви успішно записані до лікаря ${doctorName}!`);
        // Оновлюємо слоти, щоб прибрати заброньований
        handleShowAvailability(selectedDoctorId);
    };

    // 4. Головний рендер-блок (Запобіжники)

    // Стан 1: Профіль ще завантажується
    if (profileLoading) {
        return <div className="spinner-border text-primary" role="status"><span className="visually-hidden">Завантаження...</span></div>;
    }

    // Стан 2: Профіль завантажено, але не заповнено
    if (!isProfileComplete) {
        return (
            <div className="card shadow-sm text-center p-4">
                <h3 className="text-danger">Доступ обмежено</h3>
                <p className="lead">Будь ласка, спершу заповніть дані вашого профілю.</p>
                <div className="mt-2">
                    <Link to="/patient/dashboard/profile" className="btn btn-primary">
                        Перейти до профілю
                    </Link>
                </div>
            </div>
        );
    }

    // Стан 3: Профіль заповнений, завантажуємо лікарів
    if (doctorsLoading) return <p>Завантаження лікарів...</p>;

    // Стан 4: Все завантажено, відображаємо сторінку
    return (
        <div>
            <h1 className="display-5">Перегляд лікарів</h1>
            <p>Тут ви можете знайти лікаря та записатися на прийом.</p>

            {error && <div className="alert alert-danger">{error}</div>}
            {success && <div className="alert alert-success">{success}</div>}

            <div className="row">
                {doctors.length === 0 ? (
                    <p>Наразі немає доступних лікарів.</p>
                ) : (
                    doctors.map(doc => (
                        <div key={doc.id} className="col-md-6 mb-4">
                            <div className="card">
                                <div className="card-body">
                                    <h5 className="card-title">{doc.fullName}</h5>
                                    <h6 className="card-subtitle mb-2 text-muted">{doc.specialization}</h6>
                                    <p className="card-text">{doc.bio}</p>
                                    <p className="card-text"><strong>Досвід:</strong> {doc.experienceYears} років</p>
                                    <p className="card-text"><strong>Рейтинг:</strong> {doc.rating?.toFixed(1) ?? 'N/A'}</p>

                                    <button
                                        className="btn btn-primary"
                                        onClick={() => handleShowAvailability(doc.id)}
                                    >
                                        {selectedDoctorId === doc.id ? 'Сховати розклад' : 'Показати розклад'}
                                    </button>

                                    {selectedDoctorId === doc.id && (
                                        <div className="mt-3">
                                            {loadingSlots && <p>Завантаження слотів...</p>}
                                            {!loadingSlots && availability.length === 0 && <p>Вільних слотів немає.</p>}
                                            <div className="d-flex flex-wrap gap-2">
                                                {availability.map(slot => (
                                                    <button
                                                        key={slot.id}
                                                        className="btn btn-outline-success btn-sm"
                                                        onClick={() => setModalSlot(slot)}
                                                    >
                                                        {new Date(slot.availableDate).toLocaleDateString()} {slot.startTime.substring(0, 5)}
                                                    </button>
                                                ))}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {/* Рендер модального вікна, якщо слот обрано */}
            {modalSlot && (
                <BookingModal
                    doctor={doctors.find(d => d.id === selectedDoctorId)}
                    slot={modalSlot}
                    onClose={() => setModalSlot(null)}
                    onBookingSuccess={handleBookingSuccess}
                />
            )}
        </div>
    );
}