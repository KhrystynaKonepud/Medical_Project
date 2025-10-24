import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import ConfirmationModal from './ConfirmationModal'; // імпортуємо модалку

const AdminDoctors = () => {
    const [doctors, setDoctors] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [modalOpen, setModalOpen] = useState(false);
    const [modalMessage, setModalMessage] = useState('');
    const [isErrorModal, setIsErrorModal] = useState(false);
    const [selectedDoctorId, setSelectedDoctorId] = useState(null);

    const navigate = useNavigate();

    // Завантаження лікарів
    useEffect(() => {
        const fetchDoctors = async () => {
            try {
                const response = await axios.get('/api/admin/doctors');
                setDoctors(response.data);
            } catch (err) {
                console.error("Помилка завантаження лікарів:", err);
                setError("Не вдалося завантажити список лікарів.");
            } finally {
                setLoading(false);
            }
        };

        fetchDoctors();
    }, []);

    // Відкриття модалки підтвердження видалення
    const confirmDeleteDoctor = (doctorId) => {
        setSelectedDoctorId(doctorId);
        setModalMessage(`Ви впевнені, що хочете видалити лікаря ID ${doctorId}?`);
        setIsErrorModal(false);
        setModalOpen(true);
    };

    // Підтвердження видалення
    const handleConfirmDelete = async () => {
        if (!selectedDoctorId) return;

        setLoading(true);
        setModalOpen(false);

        try {
            await axios.delete(`/api/admin/doctors/${selectedDoctorId}`);
            setDoctors(prev => prev.filter(d => d.id !== selectedDoctorId));
        } catch (err) {
            console.error("Помилка видалення лікаря:", err);
            setModalMessage(`Помилка при видаленні лікаря ID ${selectedDoctorId}.`);
            setIsErrorModal(true);
            setModalOpen(true);
        } finally {
            setLoading(false);
            setSelectedDoctorId(null);
        }
    };

    // Закриття модалки
    const handleCancelModal = () => {
        setModalOpen(false);
        setSelectedDoctorId(null);
    };

    if (loading) {
        return (
            <div className="container mt-4 text-center">
                <p>Завантаження списку лікарів...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="container mt-4">
                <div className="alert alert-danger" role="alert">{error}</div>
            </div>
        );
    }

    return (

        <div className="container mt-4">
            <button
                onClick={() => navigate('/admin/dashboard')}
                className="btn btn-secondary"
                disabled={loading}
            >
                Назад на головне меню
            </button>
            <h1 className="display-5 mb-4">Список лікарів</h1>

            {doctors.length === 0 ? (
                <div className="alert alert-info" role="alert">
                    Наразі лікарів не знайдено. Будь ласка, додайте нового лікаря.
                </div>
            ) : (
                <div className="row">
                    {doctors.map(doctor => (
                        <div key={doctor.id} className="col-lg-6 col-md-12 mb-4">
                            <div className="card shadow-sm h-100">
                                <div className="card-body">
                                    <h5 className="card-title text-primary">{doctor.fullName || "Ім'я не вказано"}</h5>
                                    <p className="card-text">
                                        <strong>Спеціалізація:</strong> {doctor.specialization}<br />
                                        <strong>Досвід:</strong> {doctor.experienceYears} років<br />
                                        <strong>Email:</strong> {doctor.email}<br />
                                        <strong>Про лікаря:</strong> {doctor.bio || "Немає опису"}
                                    </p>

                                    <button
                                        onClick={() => navigate(`/admin/editdoctor/${doctor.id}`)}
                                        className="btn btn-sm btn-outline-secondary"
                                        disabled={loading}
                                    >
                                        Редагувати
                                    </button>

                                    <button
                                        onClick={() => confirmDeleteDoctor(doctor.id)}
                                        className="btn btn-sm btn-danger ml-2"
                                        disabled={loading}
                                    >
                                        Видалити
                                    </button>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <button
                onClick={() => navigate('/admin/createdoctor')}
                className="btn btn-success mt-4"
                disabled={loading}
            >
                Додати нового лікаря
            </button>

            {/* Модалка */}
            <ConfirmationModal
                isOpen={modalOpen}
                message={modalMessage}
                onConfirm={handleConfirmDelete}
                onCancel={handleCancelModal}
            />
        </div>
    );
};

export default AdminDoctors;