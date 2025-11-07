import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const DoctorPrescriptions = () => {
    const navigate = useNavigate();

    const [prescriptions, setPrescriptions] = useState([]);
    const [patients, setPatients] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState('');

    // Фільтр
    const [selectedPatient, setSelectedPatient] = useState('');

    // Модальне вікно для створення рецепту
    const [showModal, setShowModal] = useState(false);
    const [formData, setFormData] = useState({
        patientId: '',
        medication: '',
        dosage: '',
        notes: ''
    });
    const [formErrors, setFormErrors] = useState({});
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        fetchData();
    }, [selectedPatient]);

    const fetchData = async () => {
        try {
            setLoading(true);

            // Завантажуємо пацієнтів та рецепти
            const [patientsResponse, prescriptionsResponse] = await Promise.all([
                axios.get('/api/doctor/patients'),
                axios.get('/api/doctor/prescriptions', {
                    params: selectedPatient ? { patientId: selectedPatient } : {}
                })
            ]);

            setPatients(patientsResponse.data);
            setPrescriptions(prescriptionsResponse.data);
            setError(null);
        } catch (err) {
            setError('Помилка завантаження даних.');
            console.error('Error fetching data:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = () => {
        setShowModal(true);
        setFormData({
            patientId: selectedPatient || '',
            medication: '',
            dosage: '',
            notes: ''
        });
        setFormErrors({});
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setFormData({
            patientId: '',
            medication: '',
            dosage: '',
            notes: ''
        });
        setFormErrors({});
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));

        if (formErrors[name]) {
            setFormErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };

    const validateForm = () => {
        const errors = {};

        if (!formData.patientId) {
            errors.patientId = 'Оберіть пацієнта';
        }

        if (!formData.medication.trim()) {
            errors.medication = 'Вкажіть назву препарату';
        }

        if (!formData.dosage.trim()) {
            errors.dosage = 'Вкажіть дозування';
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            setSubmitting(true);
            setError(null);

            const dataToSubmit = {
                patientId: parseInt(formData.patientId),
                medication: formData.medication,
                dosage: formData.dosage,
                notes: formData.notes || null
            };

            await axios.post('/api/doctor/prescriptions', dataToSubmit);

            setSuccessMessage('Рецепт успішно створено!');
            handleCloseModal();
            fetchData();

            setTimeout(() => setSuccessMessage(''), 3000);
        } catch (err) {
            if (err.response?.data?.message) {
                setError(err.response.data.message);
            } else {
                setError('Помилка створення рецепту. Спробуйте ще раз.');
            }
            console.error('Error creating prescription:', err);
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Ви впевнені, що хочете видалити цей рецепт?')) {
            return;
        }

        try {
            await axios.delete(`/api/doctor/prescriptions/${id}`);
            setSuccessMessage('Рецепт успішно видалено!');
            fetchData();
            setTimeout(() => setSuccessMessage(''), 3000);
        } catch (err) {
            setError('Помилка видалення рецепту.');
            console.error('Error deleting prescription:', err);
        }
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleString('uk-UA', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    if (loading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Завантаження...</span>
                </div>
            </div>
        );
    }

    return (
        <div style={{ padding: '20px', maxWidth: '1400px', margin: '0 auto' }}>
            <div className="row mb-4">
                <div className="col-md-12">
                    <h2 style={{ color: '#2c3e50', fontWeight: '600' }}>
                        <i className="fas fa-prescription" style={{ marginRight: '10px', color: '#f39c12' }}></i>
                        Рецепти
                    </h2>
                </div>
            </div>

            {error && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                    <i className="fas fa-exclamation-circle"></i> {error}
                    <button type="button" className="close" onClick={() => setError(null)}>
                        <span>&times;</span>
                    </button>
                </div>
            )}

            {successMessage && (
                <div className="alert alert-success alert-dismissible fade show" role="alert">
                    <i className="fas fa-check-circle"></i> {successMessage}
                </div>
            )}

            {/* Фільтр та кнопка додавання */}
            <div className="card mb-4" style={{ boxShadow: '0 2px 4px rgba(0,0,0,0.1)', borderRadius: '8px' }}>
                <div className="card-body">
                    <div className="row align-items-end">
                        <div className="col-md-6">
                            <label className="form-label">Фільтр за пацієнтом</label>
                            <select
                                className="form-control"
                                value={selectedPatient}
                                onChange={(e) => setSelectedPatient(e.target.value)}
                                style={{ borderRadius: '6px' }}
                            >
                                <option value="">Всі пацієнти</option>
                                {patients.map(patient => (
                                    <option key={patient.id} value={patient.id}>
                                        {patient.name}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div className="col-md-6 text-end">
                            <button
                                className="btn btn-success"
                                onClick={handleOpenModal}
                                style={{ borderRadius: '6px' }}
                            >
                                <i className="fas fa-plus"></i> Додати новий рецепт
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Список рецептів */}
            {prescriptions.length > 0 ? (
                <div className="row">
                    {prescriptions.map((prescription) => (
                        <div key={prescription.id} className="col-md-6 mb-3">
                            <div className="card h-100" style={{ boxShadow: '0 2px 4px rgba(0,0,0,0.1)', borderRadius: '8px', border: '1px solid #f39c12' }}>
                                <div className="card-header bg-warning text-dark" style={{ fontWeight: '600' }}>
                                    <div className="d-flex justify-content-between align-items-center">
                                        <span>
                                            <i className="fas fa-user"></i> {prescription.patientName}
                                        </span>
                                        <small>{formatDate(prescription.dateIssued)}</small>
                                    </div>
                                </div>
                                <div className="card-body">
                                    <h5 style={{ color: '#f39c12', marginBottom: '15px' }}>
                                        <i className="fas fa-pills"></i> {prescription.medication}
                                    </h5>
                                    <p className="mb-2">
                                        <strong><i className="fas fa-weight" style={{ marginRight: '8px', color: '#3498db' }}></i>Дозування:</strong><br />
                                        {prescription.dosage}
                                    </p>
                                    {prescription.notes && (
                                        <p className="mb-2">
                                            <strong><i className="fas fa-sticky-note" style={{ marginRight: '8px', color: '#3498db' }}></i>Примітки:</strong><br />
                                            {prescription.notes}
                                        </p>
                                    )}
                                </div>
                                <div className="card-footer bg-transparent">
                                    <button
                                        className="btn btn-danger btn-sm w-100"
                                        onClick={() => handleDelete(prescription.id)}
                                        style={{ borderRadius: '6px' }}
                                    >
                                        <i className="fas fa-trash"></i> Видалити
                                    </button>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            ) : (
                <div className="alert alert-info text-center" style={{ padding: '60px 20px', borderRadius: '8px' }}>
                    <i className="fas fa-info-circle fa-3x mb-3" style={{ color: '#3498db' }}></i>
                    <h4>Рецептів не знайдено</h4>
                    <p>Натисніть "Додати новий рецепт" щоб створити перший рецепт.</p>
                </div>
            )}

            {/* Модальне вікно для створення рецепту */}
            {showModal && (
                <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
                    <div className="modal-dialog modal-dialog-centered">
                        <div className="modal-content" style={{ borderRadius: '8px' }}>
                            <div className="modal-header bg-success text-white">
                                <h5 className="modal-title">
                                    <i className="fas fa-prescription"></i> Новий рецепт
                                </h5>
                                <button type="button" className="close text-white" onClick={handleCloseModal}>
                                    <span>&times;</span>
                                </button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    <div className="form-group mb-3">
                                        <label htmlFor="patientId" className="form-label">
                                            <i className="fas fa-user"></i> Пацієнт <span className="text-danger">*</span>
                                        </label>
                                        <select
                                            id="patientId"
                                            name="patientId"
                                            className={`form-control ${formErrors.patientId ? 'is-invalid' : ''}`}
                                            value={formData.patientId}
                                            onChange={handleChange}
                                            required
                                            style={{ borderRadius: '6px' }}
                                        >
                                            <option value="">Оберіть пацієнта</option>
                                            {patients.map(patient => (
                                                <option key={patient.id} value={patient.id}>
                                                    {patient.name}
                                                </option>
                                            ))}
                                        </select>
                                        {formErrors.patientId && (
                                            <div className="invalid-feedback">{formErrors.patientId}</div>
                                        )}
                                    </div>

                                    <div className="form-group mb-3">
                                        <label htmlFor="medication" className="form-label">
                                            <i className="fas fa-pills"></i> Назва препарату <span className="text-danger">*</span>
                                        </label>
                                        <input
                                            type="text"
                                            id="medication"
                                            name="medication"
                                            className={`form-control ${formErrors.medication ? 'is-invalid' : ''}`}
                                            placeholder="Наприклад: Парацетамол"
                                            value={formData.medication}
                                            onChange={handleChange}
                                            required
                                            style={{ borderRadius: '6px' }}
                                        />
                                        {formErrors.medication && (
                                            <div className="invalid-feedback">{formErrors.medication}</div>
                                        )}
                                    </div>

                                    <div className="form-group mb-3">
                                        <label htmlFor="dosage" className="form-label">
                                            <i className="fas fa-weight"></i> Дозування та спосіб прийому <span className="text-danger">*</span>
                                        </label>
                                        <textarea
                                            id="dosage"
                                            name="dosage"
                                            className={`form-control ${formErrors.dosage ? 'is-invalid' : ''}`}
                                            rows="3"
                                            placeholder="Наприклад: 500мг, 3 рази на день після їжі"
                                            value={formData.dosage}
                                            onChange={handleChange}
                                            required
                                            style={{ borderRadius: '6px' }}
                                        />
                                        {formErrors.dosage && (
                                            <div className="invalid-feedback">{formErrors.dosage}</div>
                                        )}
                                    </div>

                                    <div className="form-group mb-3">
                                        <label htmlFor="notes" className="form-label">
                                            <i className="fas fa-sticky-note"></i> Примітки
                                        </label>
                                        <textarea
                                            id="notes"
                                            name="notes"
                                            className="form-control"
                                            rows="2"
                                            placeholder="Додаткові рекомендації (необов'язково)"
                                            value={formData.notes}
                                            onChange={handleChange}
                                            style={{ borderRadius: '6px' }}
                                        />
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button
                                        type="button"
                                        className="btn btn-secondary"
                                        onClick={handleCloseModal}
                                        disabled={submitting}
                                        style={{ borderRadius: '6px' }}
                                    >
                                        <i className="fas fa-times"></i> Скасувати
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn btn-success"
                                        disabled={submitting}
                                        style={{ borderRadius: '6px' }}
                                    >
                                        {submitting ? (
                                            <>
                                                <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                                                Збереження...
                                            </>
                                        ) : (
                                            <>
                                                <i className="fas fa-save"></i> Зберегти рецепт
                                            </>
                                        )}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default DoctorPrescriptions;