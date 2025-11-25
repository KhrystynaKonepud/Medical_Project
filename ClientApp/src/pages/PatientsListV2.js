import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const PatientsListV2 = () => {
    const [patients, setPatients] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const navigate = useNavigate();

    // Завантаження пацієнтів через API v2
    useEffect(() => {
        const fetchPatients = async () => {
            try {
                const response = await axios.get('/api/v2/patients');
                setPatients(response.data);
            } catch (err) {
                console.error("Помилка завантаження пацієнтів:", err);
                setError("Не вдалося завантажити список пацієнтів.");
            } finally {
                setLoading(false);
            }
        };

        fetchPatients();
    }, []);

    const formatDate = (dateString) => {
        if (!dateString) return "Не було";
        const date = new Date(dateString);
        return date.toLocaleDateString('uk-UA');
    };

    if (loading) {
        return (
            <div className="container mt-4 text-center">
                <p>Завантаження списку пацієнтів...</p>
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
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h1 className="display-5">Список пацієнтів (API v2)</h1>
                <button
                    onClick={() => navigate('/')}
                    className="btn btn-secondary"
                >
                    На головну
                </button>
            </div>

            <div className="alert alert-success mb-4" role="alert">
                <strong>API версія 2.0:</strong> Відображає розширену інформацію про пацієнтів з медичною статистикою (кількість призначень, записів, рецептів, тощо)
            </div>

            {patients.length === 0 ? (
                <div className="alert alert-warning" role="alert">
                    Наразі пацієнтів не знайдено.
                </div>
            ) : (
                <div className="row">
                    {patients.map(patient => (
                        <div key={patient.id} className="col-lg-6 col-md-12 mb-4">
                            <div className="card shadow-sm h-100">
                                <div className="card-header bg-primary text-white">
                                    <h5 className="mb-0">{patient.fullName || "Ім'я не вказано"}</h5>
                                </div>
                                <div className="card-body">
                                    <div className="mb-3">
                                        <p className="mb-1">
                                            <strong>Email:</strong> {patient.email || "Email не вказано"}
                                        </p>
                                        <p className="mb-1">
                                            <strong>Телефон:</strong> {patient.phoneNumber || "Телефон не вказано"}
                                        </p>
                                        <p className="mb-1">
                                            <strong>Екстрений контакт:</strong> {patient.emergencyContact || "Не вказано"}
                                        </p>
                                    </div>

                                    <hr />

                                    <h6 className="text-muted mb-3">Медична статистика:</h6>
                                    <div className="row text-center">
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-primary">
                                                    {patient.statistics?.totalAppointments || 0}
                                                </div>
                                                <small className="text-muted">Призначень</small>
                                            </div>
                                        </div>
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-success">
                                                    {patient.statistics?.totalMedicalRecords || 0}
                                                </div>
                                                <small className="text-muted">Мед. записів</small>
                                            </div>
                                        </div>
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-warning">
                                                    {patient.statistics?.totalPrescriptions || 0}
                                                </div>
                                                <small className="text-muted">Рецептів</small>
                                            </div>
                                        </div>
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-info">
                                                    {patient.statistics?.totalTestResults || 0}
                                                </div>
                                                <small className="text-muted">Результатів тестів</small>
                                            </div>
                                        </div>
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-secondary">
                                                    {patient.statistics?.totalVaccinations || 0}
                                                </div>
                                                <small className="text-muted">Вакцинацій</small>
                                            </div>
                                        </div>
                                        <div className="col-6 mb-2">
                                            <div className="p-2 bg-light rounded">
                                                <div className="h4 mb-0 text-danger">
                                                    {patient.statistics?.totalReviews || 0}
                                                </div>
                                                <small className="text-muted">Відгуків</small>
                                            </div>
                                        </div>
                                    </div>

                                    <hr />

                                    <p className="mb-0">
                                        <strong>Останнє призначення:</strong>{' '}
                                        {formatDate(patient.statistics?.lastAppointmentDate)}
                                    </p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <div className="mt-4">
                <button
                    onClick={() => navigate('/patients-v1')}
                    className="btn btn-outline-primary"
                >
                    Перейти до базової версії (API v1)
                </button>
            </div>
        </div>
    );
};

export default PatientsListV2;
