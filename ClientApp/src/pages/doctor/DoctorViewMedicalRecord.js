import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";

const DoctorViewMedicalRecord = () => {
    const { appointmentId } = useParams();
    const navigate = useNavigate();
    const [record, setRecord] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        fetchRecord();
    }, [appointmentId]);

    const fetchRecord = async () => {
        try {
            setLoading(true);
            const response = await axios.get(
                `/api/doctor/medical-records/appointment/${appointmentId}`
            );
            setRecord(response.data);
            setError("");
        } catch (err) {
            console.error("Failed to fetch medical record:", err);
            setError("Не вдалося завантажити медичний запис. Перевірте підключення або наявність даних.");
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return "Не вказано";
        try {
            const date = new Date(dateString);
            if (isNaN(date)) return dateString;
            return date.toLocaleString('uk-UA', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch {
            return dateString;
        }
    };

    return (
        <div className="container mt-4">
            <h1 className="display-5 mb-4">
                <i className="fas fa-file-medical" style={{ marginRight: '10px', color: '#007bff' }}></i>
                Медичний запис
            </h1>

            <button
                className="btn btn-outline-secondary mb-3"
                onClick={() => navigate(-1)}
            >
                <i className="fas fa-arrow-left"></i> Назад
            </button>

            <div className="card shadow-sm mb-4">
                <div className="card-header bg-primary text-white">
                    <h5 className="mb-0">
                        <i className="fas fa-notes-medical"></i> Деталі медичного запису
                    </h5>
                </div>
                <div className="card-body">
                    {loading && (
                        <div className="text-center py-5">
                            <div className="spinner-border text-primary" role="status">
                                <span className="visually-hidden">Завантаження...</span>
                            </div>
                            <p className="mt-2 text-muted">Завантаження медичного запису...</p>
                        </div>
                    )}

                    {error && (
                        <div className="alert alert-warning" role="alert">
                            <i className="fas fa-exclamation-triangle"></i> {error}
                        </div>
                    )}

                    {record && !loading && (
                        <div>
                            <ul className="list-group list-group-flush">
                                <li className="list-group-item">
                                    <strong>
                                        <i className="fas fa-user" style={{ marginRight: '8px', color: '#007bff' }}></i>
                                        Пацієнт:
                                    </strong> {record.patientName || "Не вказано"}
                                </li>
                                <li className="list-group-item">
                                    <strong>
                                        <i className="fas fa-calendar-alt" style={{ marginRight: '8px', color: '#007bff' }}></i>
                                        Дата запису:
                                    </strong> {formatDate(record.recordDate)}
                                </li>
                                <li className="list-group-item">
                                    <strong>
                                        <i className="fas fa-stethoscope" style={{ marginRight: '8px', color: '#007bff' }}></i>
                                        Діагноз:
                                    </strong>
                                    <div className="mt-2 p-3" style={{
                                        backgroundColor: '#f8f9fa',
                                        borderRadius: '6px',
                                        border: '1px solid #dee2e6'
                                    }}>
                                        {record.diagnosis || "Не вказано"}
                                    </div>
                                </li>
                                <li className="list-group-item">
                                    <strong>
                                        <i className="fas fa-pills" style={{ marginRight: '8px', color: '#007bff' }}></i>
                                        Лікування:
                                    </strong>
                                    <div className="mt-2 p-3" style={{
                                        backgroundColor: '#f8f9fa',
                                        borderRadius: '6px',
                                        border: '1px solid #dee2e6'
                                    }}>
                                        {record.treatment || "Не вказано"}
                                    </div>
                                </li>
                                <li className="list-group-item">
                                    <strong>
                                        <i className="fas fa-sticky-note" style={{ marginRight: '8px', color: '#007bff' }}></i>
                                        Додаткові рекомендації:
                                    </strong>
                                    <div className="mt-2 p-3" style={{
                                        backgroundColor: '#f8f9fa',
                                        borderRadius: '6px',
                                        border: '1px solid #dee2e6'
                                    }}>
                                        {record.notes || "Немає додаткових рекомендацій"}
                                    </div>
                                </li>
                            </ul>

                            <div className="mt-4 d-flex gap-2">
                                <button
                                    className="btn btn-primary btn-lg"
                                    onClick={() => navigate(`/doctor/dashboard/medical-records-edit/${record.id}`)}
                                >
                                    <i className="fas fa-edit"></i> Редагувати медичний запис
                                </button>
                                <button
                                    className="btn btn-outline-secondary btn-lg"
                                    onClick={() => navigate(-1)}
                                >
                                    <i className="fas fa-arrow-left"></i> Назад
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default DoctorViewMedicalRecord;