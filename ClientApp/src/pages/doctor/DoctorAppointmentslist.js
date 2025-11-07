import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

const AppointmentsList = () => {
    const [appointments, setAppointments] = useState([]);
    const [statistics, setStatistics] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({
        status: '',
        dateFrom: '',
        dateTo: ''
    });

    useEffect(() => {
        fetchAppointments();
        fetchStatistics();
    }, [filters]);

    const fetchAppointments = async () => {
        try {
            setLoading(true);
            const params = new URLSearchParams();

            if (filters.status) params.append('status', filters.status);
            if (filters.dateFrom) params.append('dateFrom', filters.dateFrom);
            if (filters.dateTo) params.append('dateTo', filters.dateTo);

            const response = await axios.get(`/api/doctor/appointments?${params.toString()}`);
            setAppointments(response.data);
            setError(null);
        } catch (err) {
            setError('Помилка завантаження записів. Спробуйте пізніше.');
            console.error('Error fetching appointments:', err);
        } finally {
            setLoading(false);
        }
    };

    const fetchStatistics = async () => {
        try {
            const response = await axios.get('/api/doctor/appointments/statistics');
            setStatistics(response.data);
        } catch (err) {
            console.error('Error fetching statistics:', err);
        }
    };

    const handleFilterChange = (e) => {
        const { name, value } = e.target;
        setFilters(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const resetFilters = () => {
        setFilters({
            status: '',
            dateFrom: '',
            dateTo: ''
        });
    };

    const getStatusBadgeClass = (status) => {
        switch (status) {
            case 'Scheduled':
                return 'badge-primary';
            case 'Completed':
                return 'badge-success';
            case 'Canceled':
                return 'badge-secondary';
            default:
                return 'badge-info';
        }
    };

    const getStatusText = (status) => {
        switch (status) {
            case 'Scheduled':
                return 'Заплановано';
            case 'Completed':
                return 'Завершено';
            case 'Canceled':
                return 'Скасовано';
            default:
                return status;
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

    if (loading && !appointments.length) {
        return (
            <div className="loading-container">
                <div className="spinner-border text-primary" role="status">
                    <span className="sr-only">Завантаження...</span>
                </div>
            </div>
        );
    }

    return (
        <div className="appointments-container">
            <div className="page-header">
                <h2>
                    <i className="fas fa-calendar-check"></i> Мої записи на прийом
                </h2>
            </div>

            {error && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                    <i className="fas fa-exclamation-circle"></i> {error}
                    <button type="button" className="close" onClick={() => setError(null)}>
                        <span>&times;</span>
                    </button>
                </div>
            )}

            {/* Статистика */}
            {statistics && (
                <div className="row mb-4">
                    <div className="col-md-3">
                        <div className="stat-card bg-primary text-white">
                            <div className="stat-icon">
                                <i className="fas fa-calendar-day"></i>
                            </div>
                            <div className="stat-content">
                                <h3>{statistics.todayAppointments}</h3>
                                <p>Сьогодні</p>
                            </div>
                        </div>
                    </div>
                    <div className="col-md-3">
                        <div className="stat-card bg-info text-white">
                            <div className="stat-icon">
                                <i className="fas fa-clock"></i>
                            </div>
                            <div className="stat-content">
                                <h3>{statistics.scheduledAppointments}</h3>
                                <p>Заплановано</p>
                            </div>
                        </div>
                    </div>
                    <div className="col-md-3">
                        <div className="stat-card bg-success text-white">
                            <div className="stat-icon">
                                <i className="fas fa-check-circle"></i>
                            </div>
                            <div className="stat-content">
                                <h3>{statistics.completedAppointments}</h3>
                                <p>Завершено</p>
                            </div>
                        </div>
                    </div>
                    <div className="col-md-3">
                        <div className="stat-card bg-warning text-dark">
                            <div className="stat-icon">
                                <i className="fas fa-calendar-week"></i>
                            </div>
                            <div className="stat-content">
                                <h3>{statistics.thisWeekAppointments}</h3>
                                <p>Цього тижня</p>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Фільтри */}
            <div className="card filters-card mb-4">
                <div className="card-body">
                    <div className="row">
                        <div className="col-md-4">
                            <label className="form-label">Статус</label>
                            <select
                                name="status"
                                className="form-control"
                                value={filters.status}
                                onChange={handleFilterChange}
                            >
                                <option value="">Всі статуси</option>
                                <option value="Scheduled">Заплановано</option>
                                <option value="Completed">Завершено</option>
                                <option value="Canceled">Скасовано</option>
                            </select>
                        </div>
                        <div className="col-md-3">
                            <label className="form-label">Дата від</label>
                            <input
                                type="date"
                                name="dateFrom"
                                className="form-control"
                                value={filters.dateFrom}
                                onChange={handleFilterChange}
                            />
                        </div>
                        <div className="col-md-3">
                            <label className="form-label">Дата до</label>
                            <input
                                type="date"
                                name="dateTo"
                                className="form-control"
                                value={filters.dateTo}
                                onChange={handleFilterChange}
                            />
                        </div>
                        <div className="col-md-2 d-flex align-items-end">
                            <button
                                type="button"
                                className="btn btn-secondary w-100"
                                onClick={resetFilters}
                            >
                                <i className="fas fa-times"></i> Скинути
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Список записів */}
            {appointments.length > 0 ? (
                <div className="row">
                    {appointments.map((appointment) => (
                        <div key={appointment.id} className="col-md-6 mb-3">
                            <div className={`card appointment-card h-100 border-${appointment.status === 'Completed' ? 'success' :
                                appointment.status === 'Scheduled' ? 'primary' : 'secondary'
                                }`}>
                                <div className={`card-header bg-${appointment.status === 'Completed' ? 'success' :
                                    appointment.status === 'Scheduled' ? 'primary' : 'secondary'
                                    } text-white`}>
                                    <div className="d-flex justify-content-between align-items-center">
                                        <h5 className="mb-0">
                                            <i className="fas fa-user"></i> {appointment.patientName}
                                        </h5>
                                        <span className={`badge ${getStatusBadgeClass(appointment.status)}`}>
                                            {getStatusText(appointment.status)}
                                        </span>
                                    </div>
                                </div>
                                <div className="card-body">
                                    <p className="mb-2">
                                        <strong><i className="fas fa-calendar"></i> Дата і час:</strong><br />
                                        {formatDate(appointment.date)}
                                    </p>
                                    <p className="mb-2">
                                        <strong><i className="fas fa-phone"></i> Телефон:</strong><br />
                                        {appointment.patientPhone}
                                    </p>
                                    <p className="mb-2">
                                        <strong><i className="fas fa-envelope"></i> Email:</strong><br />
                                        {appointment.patientEmail}
                                    </p>
                                    {appointment.reason && (
                                        <p className="mb-2">
                                            <strong><i className="fas fa-notes-medical"></i> Причина візиту:</strong><br />
                                            {appointment.reason}
                                        </p>
                                    )}
                                </div>
                                <div className="card-footer bg-transparent">
                                    {appointment.hasMedicalRecord ? (
                                        <Link
                                            to={`/doctor/dashboard/medicalrecord/${appointment.id}`}

                                            className="btn btn-info btn-sm w-100"
                                        >
                                            <i className="fas fa-eye"></i> Переглянути запис
                                        </Link>
                                    ) : appointment.status === 'Scheduled' ? (
                                        <Link
                                            to={`/doctor/dashboard/createrecord/${appointment.id}`}

                                            className="btn btn-success btn-sm w-100"
                                        >
                                            <i className="fas fa-plus"></i> Створити медичний запис
                                        </Link>
                                    ) : (
                                        <button className="btn btn-secondary btn-sm w-100" disabled>
                                            <i className="fas fa-ban"></i> Недоступно
                                        </button>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            ) : (
                <div className="alert alert-info text-center empty-state">
                    <i className="fas fa-info-circle fa-3x mb-3"></i>
                    <h4>Записів не знайдено</h4>
                    <p>У вас поки немає призначених записів на прийом або записи не відповідають обраним фільтрам.</p>
                </div>
            )}
        </div>
    );
};

export default AppointmentsList;