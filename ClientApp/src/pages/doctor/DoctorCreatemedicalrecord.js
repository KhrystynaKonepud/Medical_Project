import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';

const CreateMedicalRecord = () => {
    const { appointmentId } = useParams();
    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState('');

    const [appointmentData, setAppointmentData] = useState(null);
    const [formData, setFormData] = useState({
        diagnosis: '',
        treatment: '',
        notes: ''
    });

    const [formErrors, setFormErrors] = useState({});

    useEffect(() => {
        fetchAppointmentData();
    }, [appointmentId]);

    const fetchAppointmentData = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`/api/doctor/appointments/${appointmentId}`);
            setAppointmentData(response.data);
            setError(null);
        } catch (err) {
            console.error('Error fetching appointment:', err);
            setError('Помилка завантаження даних про запис.');
        } finally {
            setLoading(false);
        }
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

        if (!formData.diagnosis.trim()) {
            errors.diagnosis = 'Діагноз є обов\'язковим полем';
        }

        if (!formData.treatment.trim()) {
            errors.treatment = 'Лікування є обов\'язковим полем';
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        if (submitting) {
            return;
        }

        try {
            setSubmitting(true);
            setError(null);

            const dataToSubmit = {
                appointmentId: parseInt(appointmentId),
                patientId: appointmentData.patientId,
                title: `Appointment_${appointmentId}`,
                diagnosis: formData.diagnosis.trim(),
                treatment: formData.treatment.trim(),
                notes: formData.notes.trim() || null
            };

            console.log('Submitting medical record...', dataToSubmit);

            // POST запит
            const response = await axios.post('/api/doctor/medical-records', dataToSubmit);

            console.log('Medical record created successfully:', response.data);

            // Якщо дійшли сюди - все ОК
            setSuccessMessage('Медичний запис успішно створено!');

            // Перенаправлення через 1.5 секунди
            setTimeout(() => {
                navigate('/doctor/dashboard/appointmentslist', { replace: true });
            }, 1500);
        } catch (err) {
            console.error('Error creating medical record:', err);

            // Показуємо помилку
            if (err.response?.data?.message) {
                setError(err.response.data.message);
            } else if (err.response?.status === 401) {
                setError('Помилка авторизації. Будь ласка, увійдіть знову.');
            } else if (err.response?.status === 400) {
                setError('Помилка валідації даних. Перевірте введені дані.');
            } else {
                setError('Помилка створення медичного запису. Спробуйте ще раз.');
            }

            setSubmitting(false); // Дозволяємо повторну спробу
        }
        // НЕ додаємо finally з setSubmitting(false) - це заважає перенаправленню
    };

    if (loading) {
        return (
            <div className="loading-container" style={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: '400px'
            }}>
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Завантаження...</span>
                </div>
            </div>
        );
    }

    if (!appointmentData) {
        return (
            <div className="container mt-4">
                <div className="alert alert-danger">
                    <i className="fas fa-exclamation-circle"></i> Запис не знайдено
                </div>
                <button className="btn btn-secondary" onClick={() => navigate('/doctor/patients')}>
                    <i className="fas fa-arrow-left"></i> Назад до записів
                </button>
            </div>
        );
    }

    return (
        <div className="create-medical-record-container container mt-4">
            <div className="row mb-4">
                <div className="col-md-12">
                    <button
                        className="btn btn-secondary mb-3"
                        onClick={() => navigate('/doctor/patients')}
                        disabled={submitting}
                    >
                        <i className="fas fa-arrow-left"></i> Назад до записів
                    </button>
                    <h2>
                        <i className="fas fa-file-medical"></i> Створити медичний запис
                    </h2>
                </div>
            </div>

            {error && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                    <i className="fas fa-exclamation-circle"></i> {error}
                    <button
                        type="button"
                        className="btn-close"
                        onClick={() => setError(null)}
                    ></button>
                </div>
            )}

            {successMessage && (
                <div className="alert alert-success" role="alert">
                    <i className="fas fa-check-circle"></i> {successMessage}
                    <div className="mt-2">
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Перенаправлення...
                    </div>
                </div>
            )}

            <div className="row">
                <div className="col-md-8">
                    {/* Інформація про пацієнта */}
                    <div className="card patient-info-card mb-4">
                        <div className="card-header bg-primary text-white">
                            <h5 className="mb-0">
                                <i className="fas fa-user"></i> Інформація про пацієнта
                            </h5>
                        </div>
                        <div className="card-body">
                            <div className="row">
                                <div className="col-md-6">
                                    <p><strong>Пацієнт:</strong> {appointmentData.patientName}</p>
                                    <p><strong>Телефон:</strong> {appointmentData.patientPhone}</p>
                                </div>
                                <div className="col-md-6">
                                    <p><strong>Email:</strong> {appointmentData.patientEmail}</p>
                                    <p><strong>Дата прийому:</strong> {new Date(appointmentData.date).toLocaleString('uk-UA')}</p>
                                </div>
                            </div>
                            {appointmentData.reason && (
                                <div className="mt-2">
                                    <p><strong>Причина візиту:</strong> {appointmentData.reason}</p>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Форма медичного запису */}
                    <form onSubmit={handleSubmit}>
                        <div className="card medical-record-form-card">
                            <div className="card-header bg-success text-white">
                                <h5 className="mb-0">Медичний запис</h5>
                            </div>
                            <div className="card-body">
                                <div className="form-group mb-3">
                                    <label htmlFor="diagnosis" className="form-label">
                                        <i className="fas fa-stethoscope"></i> Діагноз <span className="text-danger">*</span>
                                    </label>
                                    <textarea
                                        id="diagnosis"
                                        name="diagnosis"
                                        className={`form-control ${formErrors.diagnosis ? 'is-invalid' : ''}`}
                                        rows="4"
                                        placeholder="Введіть діагноз пацієнта"
                                        value={formData.diagnosis}
                                        onChange={handleChange}
                                        disabled={submitting}
                                        required
                                    />
                                    {formErrors.diagnosis && (
                                        <div className="invalid-feedback">{formErrors.diagnosis}</div>
                                    )}
                                    <div className="form-text">Детальний опис встановленого діагнозу</div>
                                </div>

                                <div className="form-group mb-3">
                                    <label htmlFor="treatment" className="form-label">
                                        <i className="fas fa-heartbeat"></i> Призначене лікування <span className="text-danger">*</span>
                                    </label>
                                    <textarea
                                        id="treatment"
                                        name="treatment"
                                        className={`form-control ${formErrors.treatment ? 'is-invalid' : ''}`}
                                        rows="5"
                                        placeholder="Опишіть призначене лікування"
                                        value={formData.treatment}
                                        onChange={handleChange}
                                        disabled={submitting}
                                        required
                                    />
                                    {formErrors.treatment && (
                                        <div className="invalid-feedback">{formErrors.treatment}</div>
                                    )}
                                    <div className="form-text">План лікування та рекомендації</div>
                                </div>

                                <div className="form-group mb-3">
                                    <label htmlFor="notes" className="form-label">
                                        <i className="fas fa-sticky-note"></i> Додаткові примітки
                                    </label>
                                    <textarea
                                        id="notes"
                                        name="notes"
                                        className="form-control"
                                        rows="3"
                                        placeholder="Додаткова інформація (необов'язково)"
                                        value={formData.notes}
                                        onChange={handleChange}
                                        disabled={submitting}
                                    />
                                    <div className="form-text">Будь-яка додаткова інформація про стан пацієнта</div>
                                </div>

                                <div className="alert alert-info">
                                    <i className="fas fa-info-circle"></i>
                                    <strong> Важливо:</strong> Після створення медичного запису ви зможете переглянути його в списку записів пацієнта.
                                </div>

                                <div className="d-flex justify-content-end gap-2">
                                    <button
                                        type="button"
                                        className="btn btn-secondary"
                                        onClick={() => navigate('/doctor/patients')}
                                        disabled={submitting}
                                    >
                                        <i className="fas fa-times"></i> Скасувати
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn btn-success"
                                        disabled={submitting}
                                    >
                                        {submitting ? (
                                            <>
                                                <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                                                Збереження...
                                            </>
                                        ) : (
                                            <>
                                                <i className="fas fa-save"></i> Зберегти запис
                                            </>
                                        )}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>

                {/* Бокова панель з підказками */}
                <div className="col-md-4">
                    <div className="card tips-card bg-light">
                        <div className="card-header">
                            <h5 className="mb-0"><i className="fas fa-lightbulb"></i> Підказки</h5>
                        </div>
                        <div className="card-body">
                            <h6><i className="fas fa-check-circle text-success"></i> Діагноз</h6>
                            <ul className="tips-list">
                                <li>Будьте максимально точними</li>
                                <li>Використовуйте медичну термінологію</li>
                                <li>Вказуйте код МКХ-10 (якщо відомо)</li>
                            </ul>

                            <h6 className="mt-3"><i className="fas fa-check-circle text-success"></i> Лікування</h6>
                            <ul className="tips-list">
                                <li>Опишіть всі етапи лікування</li>
                                <li>Вказуйте дозування препаратів</li>
                                <li>Додайте терміни контрольних оглядів</li>
                            </ul>

                            <h6 className="mt-3"><i className="fas fa-check-circle text-success"></i> Примітки</h6>
                            <ul className="tips-list">
                                <li>Алергічні реакції</li>
                                <li>Супутні захворювання</li>
                                <li>Особливості пацієнта</li>
                            </ul>
                        </div>
                    </div>

                    <div className="card warning-card bg-warning bg-opacity-25 mt-3">
                        <div className="card-body">
                            <h6><i className="fas fa-exclamation-triangle"></i> Увага!</h6>
                            <p className="small mb-0">
                                Медичний запис буде збережено в історії хвороби пацієнта.
                                Переконайтеся, що вся інформація введена правильно перед збереженням.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateMedicalRecord;