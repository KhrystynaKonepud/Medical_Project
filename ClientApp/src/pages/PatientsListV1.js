import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const PatientsListV1 = () => {
    const [patients, setPatients] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const navigate = useNavigate();

    // Завантаження пацієнтів через API v1
    useEffect(() => {
        const fetchPatients = async () => {
            try {
                const response = await axios.get('/api/v1/patients');
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
                <h1 className="display-5">Список пацієнтів (API v1)</h1>
                <button
                    onClick={() => navigate('/')}
                    className="btn btn-secondary"
                >
                    На головну
                </button>
            </div>

            <div className="alert alert-info mb-4" role="alert">
                <strong>API версія 1.0:</strong> Відображає базову інформацію про пацієнтів (ім'я, email, телефон, контакт для екстрених випадків)
            </div>

            {patients.length === 0 ? (
                <div className="alert alert-warning" role="alert">
                    Наразі пацієнтів не знайдено.
                </div>
            ) : (
                <div className="table-responsive">
                    <table className="table table-striped table-hover">
                        <thead className="table-dark">
                            <tr>
                                <th>ID</th>
                                <th>Повне ім'я</th>
                                <th>Email</th>
                                <th>Телефон</th>
                                <th>Екстрений контакт</th>
                            </tr>
                        </thead>
                        <tbody>
                            {patients.map(patient => (
                                <tr key={patient.id}>
                                    <td>{patient.id}</td>
                                    <td>{patient.fullName || "Ім'я не вказано"}</td>
                                    <td>{patient.email || "Email не вказано"}</td>
                                    <td>{patient.phoneNumber || "Телефон не вказано"}</td>
                                    <td>{patient.emergencyContact || "Не вказано"}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <div className="mt-4">
                <button
                    onClick={() => navigate('/patients-v2')}
                    className="btn btn-primary"
                >
                    Перейти до розширеної версії (API v2)
                </button>
            </div>
        </div>
    );
};

export default PatientsListV1;
